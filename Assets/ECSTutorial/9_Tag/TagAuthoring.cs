using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class TagAuthoring : MonoBehaviour
{
    public enum ColorEnum {Red, Green, Blue};

    public ColorEnum colorEnum;
}

public struct RedTag : IComponentData{};//데이터 없는 컴포넌트 == Tag 
public struct GreenTag : IComponentData{};// 엔티티 쿼리를 필터링 하기위해 사용 ,
                                        // 단점은 엔티티에서 태그를 추가하고 제거할때 구조적 변경이 발생
public struct BlueTag : IComponentData{};//ZeroCostPerEntity 으로 청크에 공간차지 하지않음

public class TagBaker : Baker<TagAuthoring>
{
    public override void Bake(TagAuthoring authoring)
    {
        switch(authoring.colorEnum)
        {
            case TagAuthoring.ColorEnum.Red:
            {
                AddComponent(new RedTag{});
                break;
            }
            case TagAuthoring.ColorEnum.Green:
            {
                AddComponent(new GreenTag{});
                break;
            }
            case TagAuthoring.ColorEnum.Blue:
            {
                AddComponent(new BlueTag{});
                break;
            }
        }
    }
}

