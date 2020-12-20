using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;
using System.Net;

namespace RoboticArm
{
    /// <summary>
    /// The API controller
    /// </summary>
    [Authentication("Basic:user p@ssw0rd")]
    public class ControllerApi
    {
        private static GpioController _controller;

        const int PinMotor1Up = 23;
        const int PinMotor1Down = 22;
        const int PinMotor2Up = 21;
        const int PinMotor2RDown = 19;
        const int PinMotor3Up = 18;
        const int PinMotor3Down = 5;
        const int PinMotor4Up = 32;
        const int PinMotor4Down = 33;
        const int PinMotor5Up = 25;
        const int PinMotor5Down = 26;
        const int PinLed = 2;

        const int TimeMillisec1 = 50;
        const int TimeMillisec2 = 50;
        const int TimeMillisec3 = 50;
        const int TimeMillisec4 = 30;
        const int TimeMillisec5 = 800;

        const int MinNumberMotor = 1;
        const int MaxNumberMotor = 5;

        private static MotorDetails[] _motors;

        /// <summary>
        /// Initialize all the motors
        /// </summary>
        static public void Initialize()
        {
            _controller = new GpioController();
            _motors = new MotorDetails[MaxNumberMotor];

            _motors[0] = new MotorDetails(0, TimeMillisec1);
            _motors[0].GpioPinUp = _controller.OpenPin(PinMotor1Up, PinMode.Output);
            _motors[0].GpioPinDown = _controller.OpenPin(PinMotor1Down, PinMode.Output);
            _motors[1] = new MotorDetails(1, TimeMillisec2);
            _motors[1].GpioPinUp = _controller.OpenPin(PinMotor2Up, PinMode.Output);
            _motors[1].GpioPinDown = _controller.OpenPin(PinMotor2RDown, PinMode.Output);
            _motors[2] = new MotorDetails(2, TimeMillisec3);
            _motors[2].GpioPinUp = _controller.OpenPin(PinMotor3Up, PinMode.Output);
            _motors[2].GpioPinDown = _controller.OpenPin(PinMotor3Down, PinMode.Output);
            _motors[3] = new MotorDetails(3, TimeMillisec4);
            _motors[3].GpioPinUp = _controller.OpenPin(PinMotor4Up, PinMode.Output);
            _motors[3].GpioPinDown = _controller.OpenPin(PinMotor4Down, PinMode.Output);
            _motors[4] = new MotorDetails(4, TimeMillisec5);
            _motors[4].GpioPinUp = _controller.OpenPin(PinMotor5Up, PinMode.Output);
            _motors[4].GpioPinDown = _controller.OpenPin(PinMotor5Down, PinMode.Output);

            _controller.OpenPin(PinLed, PinMode.Output);
        }

        /// <summary>
        /// Move motor up
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("mu")]
        public void MotorUp(WebServerEventArgs e)
        {
            var paramsQuery = WebServer.DecodeParam(e.Context.Request.RawUrl);
            var motorNum = GetMotorNumber(paramsQuery);
            if (motorNum < 0)
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                return;
            }

            var timing = GetTiming(paramsQuery);
            System.Diagnostics.Debug.WriteLine($"Motor {motorNum}, Timing {timing}");
            _motors[motorNum].Up(timing);
            e.Context.Response.ContentLength64 = 0;
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        /// <summary>
        /// Move motor down
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("md")]
        public void MotorDown(WebServerEventArgs e)
        {
            var paramsQuery = WebServer.DecodeParam(e.Context.Request.RawUrl);
            var motorNum = GetMotorNumber(paramsQuery);
            if (motorNum < 0)
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                return;
            }

            var timing = GetTiming(paramsQuery);
            System.Diagnostics.Debug.WriteLine($"Motor {motorNum}, Timing {timing}");
            _motors[motorNum].Down(timing);
            e.Context.Response.ContentLength64 = 0;
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        /// <summary>
        /// Stop motor
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("ms")]
        public void MotorStop(WebServerEventArgs e)
        {
            var paramsQuery = WebServer.DecodeParam(e.Context.Request.RawUrl);
            var motorNum = GetMotorNumber(paramsQuery);
            if (motorNum < 0)
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                return;
            }

            _motors[motorNum].Stop();
            e.Context.Response.ContentLength64 = 0;
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        /// <summary>
        /// Switch on or off the led
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("led")]
        public void Led(WebServerEventArgs e)
        {
            var paramsQuery = WebServer.DecodeParam(e.Context.Request.RawUrl);
            if (paramsQuery != null && paramsQuery.Length > 0)
            {
                if (paramsQuery[0].Name == "l")
                {
                    if (paramsQuery[0].Value == "on")
                    {
                        _controller.Write(PinLed, PinValue.High);
                    }
                    else
                    {
                        _controller.Write(PinLed, PinValue.Low);
                    }
                }
            }
        }

        private int GetMotorNumber(UrlParameter[] paramsQuery)
        {
            try
            {
                foreach (var param in paramsQuery)
                {
                    if (param.Name == "p")
                    {
                        var motor = Convert.ToInt32(param.Value);
                        if ((motor >= MinNumberMotor) && (motor <= MaxNumberMotor))
                        {
                            return motor - 1;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return -1;
        }

        private int GetTiming(UrlParameter[] paramsQuery)
        {
            try
            {
                foreach (var param in paramsQuery)
                {
                    if (param.Name == "t")
                    {
                        return Convert.ToInt32(param.Value);
                    }
                }
            }
            catch (Exception)
            {
            }

            return -1;
        }
    }
}
