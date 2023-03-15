using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public class MonoFluidSystem : MonoBehaviour
{
    //우선 해슁 없이 , 접촉(일정 범위안)하고 있는거 충돌 반사각을 평균내서 이동

    public class HashMap
    {
        public List<int> Key;
        public List<int> Value;

        public HashMap(int Capacity = 0)
        {
            Key = new List<int>(Capacity);
            Value = new List<int>(Capacity);
        }

        public void Add(int key, int value)
        {
            Key.Add(key);
            Value.Add(value);
        }
        public void AddHash( Vector3 pos, float radius, int value)
        {
            Add(GridHash.Hash(pos, radius), value);
        }
        public bool TryGetFirstValue(int key, out int value)
        {
            value = Key.Find(t => key == t);
            return value >= 0;
        }
        public bool TryGetValue(int key, out List<int> values)
        {
            values = Key.FindAll(t => key == t);
            return values.Count > 0;
        }
    }
    private struct SPHParticle
    {
        public Vector3 position;

        public Vector3 velocity;
        public Vector3 Force;
        public Vector3 Acc;//가속
        public Vector3 Penetration;//겹친거리

        public bool IsSleep;
        public bool IsGround;
        public int parameterID;

        //public GameObject go;



        public void Init(Vector3 _position, int _parameterID)//, GameObject _go)
        {
            position = _position;
            parameterID = _parameterID;
            //go = _go;

            velocity = Vector3.zero;

            Force = Vector3.zero;
            Acc = Vector3.zero;
            Penetration = Vector3.zero;
            //forceHeading = Vector3.zero;
            //density = 0.0f;
            //pressure = 0.0f;
            IsSleep = false;
            IsGround = false;
        }
        public void Debugging(int index, int Length, string text = "")
        {
            /*
            if (index == Length - 1)//Length - 1
                Debug.Log(text + " | velocity :" + velocity + " / Force : " + Force 
                    + " / Acc : " + Acc + "\n Is Sleep : " + IsSleep);*/
        }
    }
    [System.Serializable]
    private struct SPHParameters
    {
        public float particleRadius;
        public float smoothingRadius;
        public float restDensity;
        public float gravityMult;
        public float particleMass;
        public float particleViscosity;
        public float particleDrag;
    }

    #region Job
    struct HashPosition : IJobParallelFor
    {
        public NativeArray<SPHParticle> datas;
        public float Radius;
        public NativeMultiHashMap<int, int>.ParallelWriter hashMap;

        public void Execute(int index)
        {
            int hash = GridHash.Hash(datas[index].position, Radius);
            hashMap.Add(hash, index);
        }
    }
    struct FindNeighbors : IJobParallelFor
    {
        [ReadOnly] public NativeArray<SPHParticle> datas;
        [ReadOnly] public NativeMultiHashMap<int, int> hashMap;
        [ReadOnly] public NativeArray<int> cellOffsetTable;
        [WriteOnly] public NativeArray<float3> Forces;
        [WriteOnly] public NativeArray<float3> Penetration;

        public float Radius;

        public void Execute(int index)
        {
            float3 pos = datas[index].position;
            int3 gridOffset;
            int3 gridPosition = GridHash.Quantize(pos, Radius);
            int i, j, hash;
            float3 dir = float3.zero;
            float3 penetration = float3.zero;
            bool found;

            for (int oi = 0; oi < 27; oi++)
            {
                i = oi * 3;
                gridOffset = new int3(cellOffsetTable[i], cellOffsetTable[i + 1], cellOffsetTable[i + 2]);
                hash = GridHash.Hash(gridPosition + gridOffset);
                NativeMultiHashMapIterator<int> iterator;
                found = hashMap.TryGetFirstValue(hash, out j, out iterator);
                while(found)
                {
                    float3 rij = datas[j].position;
                    rij -= pos;
                    float r = math.length(rij);

                    if (r < Radius)
                    {
                        dir += math.normalizesafe(-rij) * Mathf.Pow(10, Mathf.Lerp(2, 1, r / (Radius * 0.5f)));
                        penetration += math.normalizesafe(-rij) * (Radius - r);
                    }

                    // Next neighbor
                    found = hashMap.TryGetNextValue(out j, ref iterator);
                }
            }

            //if (found)
            {
                //SPHParticle temp = datas[index];
                //temp.Force = new Vector3(dir.x, dir.y, dir.z);
                //datas[index] = temp;
            }//읽기와 쓰기가 같이 있어 문제 생기나봄
            Forces[index] = dir;
            Penetration[index] = penetration;
        }
    }
    struct SetForce : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Forces;
        [ReadOnly] public NativeArray<float3> Penetration;

        public NativeArray<SPHParticle> datas;
        void IJobParallelFor.Execute(int index)
        {
            var temp = datas[index];
            temp.Force = Forces[index];
            temp.Penetration = Penetration[index];
            datas[index] = temp;
        }
    }
    #endregion

    // Consts
    private static readonly int[] cellOffsetTable =
{
        1, 1, 1, 1, 1, 0, 1, 1, -1, 1, 0, 1, 1, 0, 0, 1, 0, -1, 1, -1, 1, 1, -1, 0, 1, -1, -1,
        0, 1, 1, 0, 1, 0, 0, 1, -1, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, -1, 1, 0, -1, 0, 0, -1, -1,
        -1, 1, 1, -1, 1, 0, -1, 1, -1, -1, 0, 1, -1, 0, 0, -1, 0, -1, -1, -1, 1, -1, -1, 0, -1, -1, -1
    };
    private static Vector3 GRAVITY = new Vector3(0.0f, -9.81f, 0.0f);//프레임단 -0.08166 이동
    private const float DT = 0.008333f;//0.0163333f;

    // Properties
    [Header("Import")]
    [SerializeField] private GameObject character0Prefab = null;
    public float SpawnOffset = 0;

    [Header("Parameters")]
    [SerializeField] private int parameterID = 0;
    [SerializeField] private SPHParameters[] parameters = null;
    [SerializeField] float CollsionPush = 25;

    [Header("Properties")]
    [SerializeField] private int amount = 250;
    [SerializeField] private int rowSize = 16;
    [SerializeField] private float RandomPos = 1f;
    [SerializeField] private int _debugIndex = 0;
    private int DebugIndex
    {
        get 
        {
            if (_debugIndex < 0)
                return 0;
            else if (_debugIndex > amount)
                return amount - 1;
            else
                return _debugIndex;
        }
    }

    // Data
    private SPHParticle[] particles;
    GameObject[] particleObj;
    HashMap hashMap;

    void Start()
    {
        InitSPH();

        hashMap = new HashMap(amount);
    }

    // Update is called once per frame
    void Update()
    {

        //HashPositions();

        //ComputeForces();//Mono
        //ComputeColliders();
        //AddPosition();

        {
            for(int i = 0; i < particles.Length; i++)
            {
                particles[i].Acc = GRAVITY;
                Vector3 dir = Vector3.zero;
                float Ndir = 0;//중력 반발력
                float MoveResistance = 0;

                for (int j = 0; j < particles.Length; j++)
                {
                    var ij = particles[i].position - particles[j].position;
                    var sDij = (ij).sqrMagnitude;
                    if (sDij <= parameters[parameterID].particleRadius * parameters[parameterID].particleRadius)
                    {
                        dir += ij;
                        //============== 바닥이 완전히 지지가 되는지 확인 > 파티클간 충돌 에서 반발력 설정
                        // ==========  중간사이에 있는 파티클이 dir이 Normailized 되면서 위 OR 아래 선택되어서 서서히 가라앉음

                        Ndir += Vector3.Dot(ij.normalized, Vector3.up);

                        var Lmr = Vector3.Dot(ij.normalized, particles[i].velocity.normalized);
                        //if (Lmr >= 0)
                        {
                            MoveResistance += Lmr;
                        }
                    }
                }

                {
                    if (particles[i].position.y <= parameters[parameterID].particleRadius * 0.5f)
                    {
                        if (particles[i].IsGround == false)
                        {
                            particles[i].velocity = Vector3.Reflect(particles[i].velocity, Vector3.up)
                                * (1 - parameters[parameterID].particleViscosity);
                            //Ndir == 0
                        }
                        var tempVec = particles[i].Acc;
                        tempVec.y = 0;

                        particles[i].Acc = tempVec;
                        particles[i].IsGround = true;
                    }
                    else
                    {
                        particles[i].IsGround = false;
                    }
                }//바닥과 충돌

                {
                    //Vector3.Dot(dir.normalized, Vector3.up)
                    if (Mathf.Approximately(dir.sqrMagnitude, 0))
                    {

                    }else
                    {
                        if (particles[i].IsGround)
                        {

                        }else
                        {
                            //float viscosity = Vector3.Dot(dir.normalized, Vector3.up);

                            //if (MoveResistance >= 0)
                            if(Ndir >= 0)
                            {
                                particles[i].velocity += -1 * (particles[i].velocity + (particles[i].Acc * DT));
                                //------------ MoveResistance 를 사용하는걸로 
                            }

                            particles[i].IsSleep = Mathf.Approximately(Ndir, 0);// 중력 반발력
                            // ------------- 중력 반발력 개선 : Ndir 합 하는 부분에서 Vector3.Up 대신 velocity
                            //-------------- Ndir이 이동반발력으로 , velocity방향으로 이동할때 얼마나 제한을 받는지
                            //------------------ clamp01() 하고 더하기 / reflect로 제한하면 더 좋은데
                            //---- 이동반발력이 0 ~ 1 인것중 (위치차이 * 이동반발력)를 합해서 reflect의 반사 노말값으로 
                            
                            //============ 진행방향과 Ndir(중력 반발력) 비교해서 하면 되긴한데 
                            //============ 겹친만큼 Force를 주면 되지 않을까?
                        }
                    }

                    if (i == particles.Length - 1)
                    {

                        // 바운스 1
                    }

                }//파티클간 충돌 -> 가라앉는거 방지


                 //------------------------  이제 바운스값 1로 안해도 안날라 가는데 음... 충돌을 무시하네  

                {
                    if (particles[i].IsGround == false)// && particles[i].IsSleep == false)
                        particles[i].velocity += particles[i].Acc * DT;

                    particles[i].position += particles[i].velocity * DT;
                    particleObj[i].transform.position += particles[i].velocity * DT;
                }//값 적용

                if (i == DebugIndex)
                {
                    print($"Velocity : {particles[i].velocity} / Acc : {particles[i].Acc} / Dir : {dir} / Ndir : {Ndir} MoveResistance : { MoveResistance}" +
                            $"\n Pos : {particles[i].position} / Is Grand : {particles[i].IsGround} / Is Sleep : {particles[i].IsSleep}");

                    if (Mathf.Approximately(dir.sqrMagnitude, 0))
                    {
                        //print("Not Collision");
                    }
                    else
                    {
                        //print("Collision Dot : " + Vector3.Dot(dir.normalized, Vector3.up) + " / " + particles[i].Acc);
                    }
                }

            }
        }//충돌 반사각 사용 테스트 / 현제 : 즉시 멈추질 않아서 붕츠붕츠 하는디?
        // ******** 파티클간의 충돌이 바닥에 있을때 밀어냄을 영향 받지않아 , 2스택은 쌓임

        //--------------------------

        // F = M * A  | A = F / M
        // 파티클간 충돌(Force) > 단발성 힘(Force) > 지속적인 힘(Acc, Gravity) > 충돌(현제는 바닥만) >> 증감쇄 > 적용(velocity * DT)
        // 중력은 가속도에 속하지만 질량과 상관없이 같음

        //바닥에 충돌해 바운스 => Force, Acc는 0일때 , 처음에 Acc에 반사 속력을 주고 > velo += (Acc - Gravity) * DT 
        //    > Acc -= Acc * (1 - drag) * DT
        // Force는 단발성힘 (파티클간 충돌, 유저입력), Acc는 (바운스, 바람), Velocity는 결과

        //--------- 겹치는거 가능하면 이상태도 좋은데 , 대신 안 쌓임
        
        //파티클간의 충돌을 Force을 주는게 아닌 , 속도를 그냥 주면?

        
    }

    private void InitSPH()
    {
        particles = new SPHParticle[amount];
        particleObj = new GameObject[amount];

        for (int i = 0; i < amount; i++)
        {
            float jitter = (UnityEngine.Random.value * 2f - 1f) * parameters[parameterID].particleRadius * 0.1f * RandomPos;
            float x = (i % rowSize) * (1 + SpawnOffset) + UnityEngine.Random.Range(-0.1f, 0.1f) * RandomPos;
            float y = 2 + (i / rowSize) / rowSize * 1.1f * (1 + SpawnOffset);
            float z = ((i / rowSize) % rowSize) * (1 + SpawnOffset) + UnityEngine.Random.Range(-0.1f, 0.1f) * RandomPos;

            GameObject go = Instantiate(character0Prefab, gameObject.transform);
            go.transform.localScale = Vector3.one * parameters[parameterID].particleRadius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter) + gameObject.transform.position;
            go.name = "char" + i.ToString();

            particles[i].Init(new Vector3(x, y, z) + gameObject.transform.position, parameterID);//, go);
            particleObj[i] = go;
        }
    }

    void ComputeForces()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Vector3 forceDir = Vector3.zero;
            int CollisionCount = 0;

            for (int j = 0; j < particles.Length; j++)
            {
                if (i == j) continue;


                Vector3 rij = particles[j].position - particles[i].position;
                float r2 = rij.sqrMagnitude;
                float r = Mathf.Sqrt(r2);

                if (r < parameters[particles[i].parameterID].smoothingRadius)
                {
                    CollisionCount++;

                    //forceDir += Math.GetCollisionReflect(particles[i].velocity, particles[j].velocity, 1, 1).normalized;
                    forceDir += -rij.normalized * Mathf.Pow(10, Mathf.Lerp(2, 1, r / (parameters[particles[i].parameterID].particleRadius * 0.5f)));
                    // * parameters[particles[i].parameterID].particleMass;
                }

                //particles[i].Collision = (CollisionCount > 0);

                //if (particles[i].IsGround == false)
                {
                    if (CollisionCount > 0)
                    {
                        particles[i].Debugging(i, particles.Length, "Collision + Ground : " + particles[i].IsGround);

                        particles[i].Force = forceDir;
                        //.normalized * parameters[particles[i].parameterID].particleRadius * 0.5f;
                        
                    }
                    else
                    {
                        //particles[i].Debugging(i, particles.Length, "Free");

                        //particles[i].Force -= particles[i].Force * 0.75f * DT;
                            //Vector3.zero;// 나중에 감쇄추가
                    }

                    // ++++ 유저 Force 추가
                }
            }
        }
    }
    private void ComputeColliders()
    {
        for (int i = 0; i < particles.Length; i++)
        {

            //if (particles[i].go.transform.position.y <= parameters[particles[i].parameterID].particleRadius * 0.5f)
            if (particleObj[i].transform.position.y <= parameters[particles[i].parameterID].particleRadius * 0.5f)
            {
                if (particles[i].IsSleep == false)
                    particles[i].Debugging(i, particles.Length, "Floor");


                if (particles[i].IsGround == false)
                {
                    //particles[i].velocity = Vector3.Reflect(particles[i].velocity, Vector3.up) * (1 - parameters[particles[i].parameterID].particleViscosity);
                    //particles[i].velocity
                    //particles[i].velocity = Vector3.zero;
                    particles[i].velocity = -GRAVITY * DT;

                    if (particles[i].velocity.sqrMagnitude < 0.1f)//------------- 바닥과 충돌인데 다음에도 내려갈려는경우 멈추기
                    {
                        particles[i].IsSleep = true;
                        particles[i].Debugging(i, particles.Length, "Sleep");
                    }//움직임 중지
                }else
                {
                    if (Mathf.Abs(Vector3.Dot(particles[i].velocity, Vector3.down)) < 90 * Mathf.Deg2Rad)
                    {
                        particles[i].IsSleep = true;
                        particles[i].Debugging(i, particles.Length, "Sleep");//velocity가 아래일때
                    }
                }

                particles[i].IsGround = true;
            }
            else
            {
                //particles[i].Debugging(i, particles.Length, " -- ");

                particles[i].IsGround = false;
            }


            if (particleObj[i].transform.position.y <= 0)//if (particles[i].go.transform.position.y <= 0)
            {
                particles[i].IsSleep = true;
            }//움직임 중지
        }
    }//============= Intersect 도 수정하기
    void AddPosition()
    {

        for (int i = 0; i < particles.Length; i++)
        {
            //부하가 걸릴때 DT를 쓰면 느려지고 , Time.deltaTime 프레임을 스킵

            Vector3 velo = Vector3.zero;

            if (particles[i].IsSleep)
            {
                particles[i].Acc += (particles[i].Force / parameters[particles[i].parameterID].particleMass) * DT;
                //particles[i].Acc = (particles[i].Force.normalized * parameters[particles[i].parameterID].particleMass) * DT;

                velo = particles[i].Acc;
                velo.y = 0;

                particles[i].velocity = velo;
                particles[i].Acc -= particles[i].Acc * parameters[particles[i].parameterID].particleViscosity * DT;
                particles[i].Force -= particles[i].Force * parameters[particles[i].parameterID].particleViscosity * DT;
            }
            else
            {
                particles[i].velocity += (particles[i].Acc + GRAVITY) * DT;//particles[i].Force.normalized * CollsionPush + 

                velo = particles[i].Force + particles[i].velocity;
                //velo =  particles[i].velocity + particles[i].Penetration;

                particles[i].Acc -= particles[i].Acc * parameters[particles[i].parameterID].particleDrag * DT;
                particles[i].Force -= particles[i].Force * parameters[particles[i].parameterID].particleViscosity * DT;
            }

            //particles[i].go.transform.position += velo * DT;//Time.deltaTime;
            particleObj[i].transform.position += velo * DT;

            particles[i].position += velo * DT;//
                                               //+= Vector3.Lerp(Vector3.zero, force + velo, DT);


            if (particles[i].position.y > parameters[particles[i].parameterID].particleRadius * 0.5f)
            {
                //particleObj[i].transform.position += particles[i].Penetration;
                //particles[i].position += particles[i].Penetration;
            }
        }
    }

    void HashPositions()
    {
        for (int index = 0; index < particles.Length; index++)
        {
            hashMap.AddHash(particles[index].position, parameters[parameterID].particleRadius, index);


            //====================

            /*
            Unity.Mathematics.int3 gridOffset;
            int hash;
            Unity.Mathematics.int3 gridPosition = GridHash.Quantize(particles[index].position, parameters[parameterID].particleRadius);

            for (int oi = 0; oi < 27; oi++)
            {
                int i = oi * 3;
                gridOffset = new Unity.Mathematics.int3(cellOffsetTable[i], cellOffsetTable[i + 1], cellOffsetTable[i + 2]);
                hash = GridHash.Hash(gridPosition + gridOffset);
                hashMap.TryGetValue(hash, out var values);

                for (int j = 0; j < values.Count; j++)
                {

                }
            }//==== GC
            */
        }

        {
            var LhashMap = new NativeMultiHashMap<int, int>(amount, Allocator.TempJob);
            var ParticleData = new NativeArray<SPHParticle>(particles, Allocator.TempJob);
            var SumForces = new NativeArray<float3>(amount, Allocator.TempJob);
            var SumPenetrat = new NativeArray<float3>(amount, Allocator.TempJob);

            var HashPosJob = new HashPosition
            {
                datas = ParticleData,
                Radius = parameters[parameterID].particleRadius,
                hashMap = LhashMap.AsParallelWriter()
            };
            var FindNeighborsJob = new FindNeighbors
            {
                hashMap = LhashMap,
                datas = ParticleData,
                cellOffsetTable = new NativeArray<int>(cellOffsetTable, Allocator.TempJob),
                Radius = parameters[parameterID].particleRadius,
                Forces = SumForces,
                Penetration = SumPenetrat
                
            };
            var SetForceJob = new SetForce
            {
                datas = ParticleData,
                Forces = SumForces,
                Penetration = SumPenetrat
            };

            var HashPosHandle = HashPosJob.Schedule(amount, rowSize);
            var FindHandle = FindNeighborsJob.Schedule(amount, rowSize, HashPosHandle);
            var SetForceHandle = SetForceJob.Schedule(amount, rowSize, FindHandle);
            SetForceHandle.Complete();

            particles = ParticleData.ToArray();
            //particles은 변화X , ParticleData == FindNeighborsJob.datas
            print(particles[0].position + " / " + particles[0].Force
                + " / " + ParticleData[0].Force + " / " + FindNeighborsJob.datas[0].Force);
            //Debug.DrawLine(particles[0].position, particles[0].position + particles[0].Force, Color.black);
        }
    }
}
