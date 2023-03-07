using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoFluidSystem : MonoBehaviour
{
    //�켱 �ؽ� ���� , ����(���� ������)�ϰ� �ִ°� �浹 �ݻ簢�� ��ճ��� �̵�

    //��ġ�� ������

    private struct SPHParticle
    {
        public Vector3 position;

        public Vector3 velocity;
        public Vector3 Force;
        public Vector3 Acc;//����

        public bool IsSleep;
        public bool IsGround;
        public int parameterID;

        public GameObject go;



        public void Init(Vector3 _position, int _parameterID, GameObject _go)
        {
            position = _position;
            parameterID = _parameterID;
            go = _go;

            velocity = Vector3.zero;

            Force = Vector3.zero;
            Acc = Vector3.zero;
            //forceHeading = Vector3.zero;
            //density = 0.0f;
            //pressure = 0.0f;
            IsSleep = false;
            IsGround = false;
        }
        public void Debugging(int index, int Length, string text = "")
        {
            
            if (index == Length - 1)//Length - 1
                Debug.Log(text + " | velocity :" + velocity + " / Force : " + Force 
                    + " / Acc : " + Acc + "\n Is Sleep : " + IsSleep);
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

    // Consts
    private static Vector3 GRAVITY = new Vector3(0.0f, -9.81f, 0.0f);//�����Ӵ� -0.08166 �̵�
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

    // Data
    private SPHParticle[] particles;

    void Start()
    {
        InitSPH();
    }

    // Update is called once per frame
    void Update()
    {
        ComputeForces();
        ComputeColliders();
        AddPosition();

        // F = M * A  | A = F / M
        // ��ƼŬ�� �浹(Force) > �ܹ߼� ��(Force) > �������� ��(Acc, Gravity) > �浹(������ �ٴڸ�) >> ������ > ����(velocity * DT)
        // �߷��� ���ӵ��� �������� ������ ������� ����

        //�ٴڿ� �浹�� �ٿ => Force, Acc�� 0�϶� , ó���� Acc�� �ݻ� �ӷ��� �ְ� > velo += (Acc - Gravity) * DT 
        //    > Acc -= Acc * (1 - drag) * DT
        // Force�� �ܹ߼��� (��ƼŬ�� �浹, �����Է�), Acc�� (�ٿ, �ٶ�), Velocity�� ���

        //--------- ��ġ�°� �����ϸ� �̻��µ� ������ , ��� �� ����
        
        //��ƼŬ���� �浹�� Force�� �ִ°� �ƴ� , �ӵ��� �׳� �ָ�?
    }

    private void InitSPH()
    {
        particles = new SPHParticle[amount];

        for (int i = 0; i < amount; i++)
        {
            float jitter = (Random.value * 2f - 1f) * parameters[parameterID].particleRadius * 0.1f;
            float x = (i % rowSize) * (1 + SpawnOffset) + Random.Range(-0.1f, 0.1f);
            float y = 2 + (i / rowSize) / rowSize * 1.1f * (1 + SpawnOffset);
            float z = ((i / rowSize) % rowSize) * (1 + SpawnOffset) + Random.Range(-0.1f, 0.1f);

            GameObject go = Instantiate(character0Prefab, gameObject.transform);
            go.transform.localScale = Vector3.one * parameters[parameterID].particleRadius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter) + gameObject.transform.position;
            go.name = "char" + i.ToString();

            particles[i].Init(new Vector3(x, y, z) + gameObject.transform.position, parameterID, go);
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
                            //Vector3.zero;// ���߿� �����߰�
                    }

                    // ++++ ���� Force �߰�
                }
            }
        }
    }
    private void ComputeColliders()
    {
        for (int i = 0; i < particles.Length; i++)
        {

            if (particles[i].go.transform.position.y <= parameters[particles[i].parameterID].particleRadius * 0.5f)
            {
                if (particles[i].IsSleep == false)
                    particles[i].Debugging(i, particles.Length, "Floor");


                if (particles[i].IsGround == false)
                {
                    //particles[i].velocity = Vector3.Reflect(particles[i].velocity, Vector3.up) * (1 - parameters[particles[i].parameterID].particleViscosity); //------------- Lerp(-GRAVITY , Reflect, 1 - Viscosity)
                    //particles[i].velocity
                    particles[i].velocity = Vector3.zero;

                    if (particles[i].velocity.sqrMagnitude < 0.1f)//------------- �ٴڰ� �浹�ε� �������� ���������°�� ���߱�
                    {
                        particles[i].IsSleep = true;
                        particles[i].Debugging(i, particles.Length, "Sleep");
                    }//������ ����
                }else
                {
                    if (Mathf.Abs(Vector3.Dot(particles[i].velocity, Vector3.down)) < 90 * Mathf.Deg2Rad)
                    {
                        particles[i].IsSleep = true;
                        particles[i].Debugging(i, particles.Length, "Sleep");//velocity�� �Ʒ��϶�
                    }
                }

                particles[i].IsGround = true;
            }
            else
            {
                //particles[i].Debugging(i, particles.Length, " -- ");

                particles[i].IsGround = false;
            }


            if (particles[i].go.transform.position.y <= 0)
            {
                particles[i].IsSleep = true;
            }//������ ����
        }
    }//============= Intersect �� �����ϱ�
    void AddPosition()
    {

        for (int i = 0; i < particles.Length; i++)
        {
            //���ϰ� �ɸ��� DT�� ���� �������� , Time.deltaTime �������� ��ŵ

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

                particles[i].Acc -= particles[i].Acc * parameters[particles[i].parameterID].particleDrag * DT;
                particles[i].Force -= particles[i].Force * parameters[particles[i].parameterID].particleViscosity * DT;
            }

            particles[i].go.transform.position += velo * DT;//Time.deltaTime;

            particles[i].position += velo * DT;//
                                                         //+= Vector3.Lerp(Vector3.zero, force + velo, DT);

        }
    }
}
