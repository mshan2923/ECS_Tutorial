using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ChangeFilter
{
    public partial class UpdateFilterSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<FilterTypeComponent>()
                .ForEach((Entity e, in FilterTypeComponent filter) =>
                {
                    //Debug.Log("Changed : " + filter.filterData);//toString이 오류떠서

                    switch (filter.filterData)
                    {
                        case FilterData.A:
                        {
                            Debug.Log("Changed : A");
                            break;
                        }
                        case FilterData.B:
                        {
                            Debug.Log("Changed : B");
                            break;
                        }
                        case FilterData.C:
                        {
                            Debug.Log("Changed : C");
                            break;
                        }
                    }
                }).Run();

                //인스팩터에서 FilterData를 바꾸면 디버그로그 발생
        }
    }

}