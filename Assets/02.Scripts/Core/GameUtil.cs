// 게임 데이터 가공용 순수 함수 모음
using System;
using System.Threading;
using UnityEngine;

public static class GameUtil
{
    private static long _lastUID = 0;

    // 마지막 활동 시각부터 지금까지 경과한 시간(초). 상한 적용, 음수는 0으로 클램프
    public static float CalculateElapsedSeconds(long lastActiveTicks, float capSeconds)
    {
        if (lastActiveTicks <= 0)
        {
            return 0f;
        }

        long elapsedTicks = DateTime.UtcNow.Ticks - lastActiveTicks;
        float elapsedSeconds = elapsedTicks / (float)TimeSpan.TicksPerSecond;

        if (elapsedSeconds < 0f)
        {
            elapsedSeconds = 0f;
        }

        return Mathf.Min(elapsedSeconds, capSeconds);
    }

    // 방치 보상 계산: 경과한 시간(초, 상한 적용)에 초당 생산량을 곱해 반환
    public static int CalculateIdleReward(long lastActiveTicks, float productionPerSec, float capSeconds)
    {
        float elapsedSeconds = CalculateElapsedSeconds(lastActiveTicks, capSeconds);

        return Mathf.CeilToInt(elapsedSeconds * productionPerSec); //올림 처리
    }

    // UID 생성 기능
    public static long GenerateUID()
    {
        long newUID = DateTime.UtcNow.Ticks;

        // 원자적 연산으로 안전하게 ID 갱신
        while (true)
        {
            long lastUID = Volatile.Read(ref _lastUID);

            // 만약 현재 시간이 이전 ID보다 작거나 같다면 (루프가 너무 빠른 경우 포함)
            // 이전 ID + 1로 강제 설정하여 중복 방지
            long UIDToAssign = (newUID <= lastUID) ? lastUID + 1 : newUID;

            // _lastId가 내가 읽은 시점과 같다면 idToAssign으로 교체 (성공 시 루프 탈출)
            if (Interlocked.CompareExchange(ref _lastUID, UIDToAssign, lastUID) == lastUID)
            {
                return UIDToAssign;
            }
            // 그 사이 다른 스레드가 값을 바꿨다면 다시 시도
        }
    }
}