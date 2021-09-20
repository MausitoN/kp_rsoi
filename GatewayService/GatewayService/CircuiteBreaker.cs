using GatewayService.Interfaces;
using GatewayService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService
{
    public enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }

    public class CircuiteBreaker : ICircuiteBreaker
    {
        private const int ErrorsLimit = 5;
        private readonly TimeSpan _openToHalfOpenWaitTime = TimeSpan.FromSeconds(60);
        private int _errorsCount;
        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private ServiceResponseGateway _lastResponse;
        private DateTime _lastStateChangedDateUtc;

        private void Reset()
        {
            _errorsCount = 0;
            _lastResponse = null;
            _state = CircuitBreakerState.Closed;
        }

        private bool IsClosed => _state == CircuitBreakerState.Closed;

        public async Task<ServiceResponseGateway> ExecuteActionAsync(Func<Task<ServiceResponseGateway>> action)
        {
            if (IsClosed)
            {
                ServiceResponseGateway response = await action().ConfigureAwait(false);

                if ((int)response.StatusCode == 500 || (int)response.StatusCode == 503)
                {
                    TrackException(response);
                }
                else
                {
                    Reset();
                }

                return response;
            }
            else
            {
                if (_state == CircuitBreakerState.HalfOpen || IsTimerExpired())
                {
                    _state = CircuitBreakerState.HalfOpen;
                    ServiceResponseGateway response = await action().ConfigureAwait(false);

                    if ((int)response.StatusCode != 500 && (int)response.StatusCode != 503)
                    {
                        Reset();
                    }
                    else
                    {
                        response.Message = "CircuitBreaker";
                        Reopen(response);
                    }

                    return response;
                }

                return _lastResponse;
            }
        }

        private void Reopen(ServiceResponseGateway response)
        {
            _state = CircuitBreakerState.Open;
            _lastStateChangedDateUtc = DateTime.UtcNow;
            _errorsCount = 0;
            _lastResponse = response;
        }

        private bool IsTimerExpired()
        {
            return _lastStateChangedDateUtc + _openToHalfOpenWaitTime < DateTime.UtcNow;
        }

        private void TrackException(ServiceResponseGateway response)
        {
            _errorsCount++;
            Console.WriteLine($"{ _errorsCount }");
            if (_errorsCount >= ErrorsLimit)
            {
                response.Message = "CircuitBreaker";
                _lastResponse = response;
                _state = CircuitBreakerState.Open;
                _lastStateChangedDateUtc = DateTime.UtcNow;
            }
        }
    }
}
