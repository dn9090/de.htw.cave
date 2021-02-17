using System;

// based on http://habrahabr.ru/post/140274/
// this is a slightly modified version
namespace KalmanFilterSimple
{
	public class KalmanFilterSimple1D
	{
	    public double X0 { get; private set; } // predicted state
	    public double P0 { get; private set; } // predicted covariance

	    public double F { get; private set; } // factor of real value to previous real value
	    public double Q { get; private set; } // measurement noise
	    public double H { get; private set; } // factor of measured value to real value
	    public double R { get; private set; } // environment noise

	    public double State { get; private set; }
	    public double Covariance { get; private set; }

	    public KalmanFilterSimple1D(double q, double r, double f = 1, double h = 1)
	    {
	        this.Q = q;
	        this.R = r;
	        this.F = f;
	        this.H = h;
	    }

	    public void SetState(double state, double covariance)
	    {
	        this.State = state;
	        this.Covariance = covariance;
	    }

	    public void Correct(double data)
	    {
	        //time update - prediction
	        this.X0 = this.F * State;
	        this.P0 = this.F * Covariance * this.F + this.Q;

	        //measurement update - correction
	        double k = this.H * this.P0 / (this.H * this.P0 * this.H + this.R);
	        this.State = this.X0 + k * (data - this.H * this.X0);
	        this.Covariance = (1 - k * this.H) * this.P0;
	    }
	}
}
