using System;
using UnityEngine;

public class TickSystem : MonoBehaviour
{
    private int _tick = 0;
    private float _tickTimer = 0;
                                       
    private const float _secondsPerTick = 0.05f;

    public class OnTickEventArgs : EventArgs
    {
        private int _tick;

        public int Tick
        {
            get
            {
                return _tick;
            }

            set
            {
                if(value >= 0)
                {
                    _tick = value;
                }
            }
        }
    }

    private void Awake()
    {
        _tick = 0;
    }

    public static event EventHandler<OnTickEventArgs> OnTick;

    private void Update()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= _secondsPerTick)
        {
            _tickTimer -= _secondsPerTick;
            _tick++;

            if(OnTick != null)
            {
                OnTick(this, new OnTickEventArgs { Tick = _tick });
            }
        }
    }
}
