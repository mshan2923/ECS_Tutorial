using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ConnectFour
{
    public enum ConnectFourColor {None = 0, Red = 1, Blue = 2}

    public partial class PlayerConnectFourSystem : SystemBase
    {
        private PieceSpawnComponent _spawnData;
        private PieceMateriaComponent _materialData;
        private Entity _gameControllerEntity;

        // 매시 랜더러도 SharedDataComponent , unity ecs에서 기본값
        // 많은 엔티티가 동일한 매시와 머티리얼이고 그림자그린다면 시퀀스로 처리
 
        // 값을 기준으로 엔티티를 청크로 그룹화 하므로 데이터중복을 제거

        protected override void OnStartRunning()
        {
            RequireForUpdate<PieceSpawnData>();

            if (! SystemAPI.HasSingleton<PieceSpawnComponent>())
            {
                this.Enabled = false;
                return;
            }
            _gameControllerEntity = SystemAPI.GetSingletonEntity<PieceSpawnComponent>();
            _spawnData = EntityManager.GetComponentData<PieceSpawnComponent>(_gameControllerEntity);
            _materialData = EntityManager.GetComponentData<PieceMateriaComponent>(_gameControllerEntity);
            
            _spawnData.isRedTurn = true;
        }

        protected override void OnUpdate()
        {
            var spawnColumn = -1;

            #region Region_GetPlayerInput

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                spawnColumn = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                spawnColumn = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                spawnColumn = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                spawnColumn = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                spawnColumn = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                spawnColumn = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                spawnColumn = 6;
            }

            #endregion

            if (spawnColumn == -1) return;

            var newHorizontalPosition = new HorizontalPosition {Value = spawnColumn};

            var highestPosition = -1;

            Entities
                .WithSharedComponentFilter(newHorizontalPosition)// newHorizontalPosition 와 동일한 컨포넌트만
                .ForEach((in VerticalPosition verticalPosition) =>
                {
                    if (verticalPosition.Value > highestPosition)
                    {
                        highestPosition = verticalPosition.Value;
                    }
                }).WithoutBurst().Run();
                //모든 엔티티중 newHorizontalPosition와 포함하며 같고 , VerticalPosition를 가지고 있는것중 ForEach


            //나머진 그냥... 생략
            //https://gist.github.com/JohnnyTurbo/3d06c0b0f925a4b65a78cb760b062cb2

            // 7 X 6 보드판에 턴을 번갈아 가면서  같은색 4개를 먼저 쌓으면 이기는 예제


                /*
                Entities
                .WithSharedComponentFilter(columnFilter)
                .WithStructuralChanges()//RenderMesh를 수정할꺼라 구조적 변경
                .ForEach((Entity winningPieceEntity, RenderMesh renderMesh, in VerticalPosition y) =>
                {
                    if (y.Value >= lowestWinningPiece)
                    {
                        renderMesh.material = _materialData.YellowMaterial;
                        EntityManager.SetSharedComponentData(winningPieceEntity, renderMesh);
                    }
                }).WithoutBurst().Run();//버스트 없이 실행
                */
        }
    }

}