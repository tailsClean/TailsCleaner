using UnityEngine;
using System.Collections.Generic;

public class WaveTimeline
{
    //메인타이머 기준으로 현재 활성 웨이브를 찾아줄 클래스
    //웨이브는 StartTime과 EndTime을 구간으로 정의할거임.

    private IReadOnlyList<WavePlan> waves;

    public WaveTimeline(IReadOnlyList<WavePlan> _waves)
    {
        waves = _waves;
    }

    //시간에 따라 웨이브를 찾아주는 함수
    public WavePlan GetWaveByTimeSeconds(int _mainSeconds)
    {
        if(waves == null)
        { return null; }
        
        for(int i = 0; i < waves.Count; i++)
        {
            WavePlan wave = waves[i];
            if(_mainSeconds >= wave.startTimeSeconds && _mainSeconds < wave.endTimeSeconds)
            {
                return wave;
            }
        }

        return null;
    }

    //마지막 웨이브의 인덱스를 반환하는 함수
    public int GetLastWaveIndex()
    {
        if(waves == null || waves.Count == 0)
        { return -1; }

        return waves[waves.Count - 1].waveIndex;
    }
}
