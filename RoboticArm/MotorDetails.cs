using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace RoboticArm
{
    public class MotorDetails
    {
        private static DateTime dtTimeout;

        private bool _isMoving;
        private int _millisec;
        private bool _dirUp;

        public int DefaultTimeMilliseconds { get; internal set; }

        public int MotorNumber { get; internal set; }
        public GpioPin GpioPinUp;
        public GpioPin GpioPinDown;

        public MotorDetails(int motorNumber, int defaultTimeMilliseconds)
        {
            MotorNumber = motorNumber;
            DefaultTimeMilliseconds = defaultTimeMilliseconds;
        }

        public void Stop()
        {
            _isMoving = false;
        }

        public void Up(int milliseconds = -1)
        {
            _dirUp = true;
            _millisec = milliseconds;
            while (_isMoving)
            {
                // Just wait
                Thread.Sleep(1);
            }

            _isMoving = true;
            new Thread(() =>
            {
                Move();
            }).Start();
        }

        public void Down(int milliseconds = -1)
        {
            _dirUp = false;
            _millisec = milliseconds;
            _isMoving = true;
            new Thread(() =>
            {
                Move();

            }).Start();
        }

        public bool IsMoving => _isMoving;

        public bool IsDirectionUp => _dirUp && _isMoving;

        public bool IsDirectionDown => (!_dirUp) && _isMoving;

        private void Move()
        {
            dtTimeout = DateTime.UtcNow.AddMilliseconds(_millisec > 0 ? _millisec : DefaultTimeMilliseconds);

            GpioPinUp.Write(_dirUp ? PinValue.High : PinValue.Low);
            GpioPinDown.Write(_dirUp ? PinValue.Low : PinValue.High);

            while ((dtTimeout > DateTime.UtcNow) || _isMoving == false)
            {
                // Just wait;
                Thread.Sleep(1);
            }

            GpioPinUp.Write(PinValue.Low);
            GpioPinDown.Write(PinValue.Low);
            _isMoving = false;
        }
    }
}
