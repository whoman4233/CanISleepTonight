/// <summary>
/// 피격 가능한 개체 (ex. 이웃, 사물) 를 위한 interface
/// </summary>
public interface IHittable
{
    // 피격 시, 호출
    void OnHit();
}
