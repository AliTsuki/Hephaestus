using System;
using System.Collections.Generic;

namespace SharpNoise.Modules
{
    /// <summary>
    /// Noise module that maps the output value from a source module onto an
    /// arbitrary function curve.
    /// </summary>
    /// <remarks>
    /// This noise module maps the output value from the source module onto an
    /// application-defined curve.  This curve is defined by a number of
    /// control points; each control point has an input value
    /// that maps to an output value.
    ///
    /// To add the control points to this curve, call the <see cref="AddControlPoint"/>
    /// method.
    ///
    /// Since this curve is a cubic spline, an application must add a minimum
    /// of four control points to the curve.  If this is not done, the
    /// <see cref="GetValue"/> method fails.  Each control point can have any input and
    /// output value, although no two control points can have the same input
    /// value.  There is no limit to the number of control points that can be
    /// added to the curve.  
    ///
    /// This noise module requires one source module.
    /// </remarks>
    [Serializable]
    public class Curve : Module
    {
        /// <summary>
        /// This structure defines a control point.
        /// Control points are used for defining splines.
        /// </summary>
        [Serializable]
        public struct ControlPoint : IEquatable<ControlPoint>, IComparable<ControlPoint>
        {
            public readonly double InputValue;
            public readonly double OutputValue;

            public override bool Equals(object obj)
            {
                if(obj is ControlPoint)
                {
                    return this.Equals((ControlPoint)obj);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return this.InputValue.GetHashCode();
            }

            /// <summary>
            /// Compare two ControlPoints for equality
            /// </summary>
            /// <param name="other">The 'other' ControlPoint</param>
            /// <returns>Returns true, if other's InputValue equals this' InputValue</returns>
            public bool Equals(ControlPoint other)
            {
                if(this.InputValue == other.InputValue)
                {
                    return true;
                }

                return false;
            }

            // Here, too, only InputValues are taken into account
            public int CompareTo(ControlPoint other)
            {
                return this.InputValue.CompareTo(other.InputValue);
            }

            public static bool operator ==(ControlPoint a, ControlPoint b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(ControlPoint a, ControlPoint b)
            {
                return !a.Equals(b);
            }

            public static bool operator <(ControlPoint a, ControlPoint b)
            {
                return a.CompareTo(b) < 0;
            }

            public static bool operator >(ControlPoint a, ControlPoint b)
            {
                return a.CompareTo(b) > 0;
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="input">The input value</param>
            /// <param name="output">The output value</param>
            public ControlPoint(double input, double output)
            {
                this.InputValue = input;
                this.OutputValue = output;
            }
        }

        private readonly List<ControlPoint> controlPoints;

        /// <summary>
        /// Gets or sets the first source module
        /// </summary>
        public Module Source0
        {
            get { return this.SourceModules[0]; }
            set { this.SourceModules[0] = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Curve()
            : base(1)
        {
            this.controlPoints = new List<ControlPoint>();
        }

        /// <summary>
        /// Adds a control point to the curve.
        /// </summary>
        /// <param name="inputValue">The input value stored in the control point.</param>
        /// <param name="outputValue">The output value stored in the control point.</param>
        /// <remarks>
        /// No two control points have the same input value.
        /// 
        /// It does not matter which order these points are added.
        /// </remarks>
        public void AddControlPoint(double inputValue, double outputValue)
        {
            ControlPoint controlPoint = new ControlPoint(inputValue, outputValue);

            if(this.controlPoints.BinarySearch(controlPoint) > 0)
            {
                throw new ArgumentException("All ControlPoints must have unique input values.");
            }

            this.controlPoints.Add(controlPoint);
            this.controlPoints.Sort();
        }

        /// <summary>
        /// Deletes all the control points on the curve.
        /// </summary>
        public void ClearControlPoints()
        {
            this.controlPoints.Clear();
        }

        /// <summary>
        /// Gets or sets all control points on the curve
        /// </summary>
        public IEnumerable<ControlPoint> ControlPoints
        {
            get
            {
                return this.controlPoints.AsReadOnly();
            }
            set
            {
                this.controlPoints.Clear();
                this.controlPoints.AddRange(value);
            }
        }

        /// <summary>
        /// Gets the number of control points on the curve
        /// </summary>
        /// <returns>Returns the number of control points on the curve</returns>
        public int ControlPointCount
        {
            get
            {
                return this.controlPoints.Count;
            }
        }

        /// <summary>
        /// See the documentation on the base class.
        /// <seealso cref="Module"/>
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>Returns the computed value</returns>
        public override double GetValue(double x, double y, double z)
        {
            // Get the output value from the source module.
            double sourceValue = this.SourceModules[0].GetValue(x, y, z);

            // Find the first element in the control point array that has an input value
            // larger than the output value from the source module.
            int indexPos;
            for(indexPos = 0; indexPos < this.controlPoints.Count; indexPos++)
            {
                if(sourceValue < this.controlPoints[indexPos].InputValue)
                {
                    break;
                }
            }

            // Find the four nearest control points so that we can perform cubic
            // interpolation.
            int index0 = NoiseMath.Clamp(indexPos - 2, 0, this.controlPoints.Count - 1);
            int index1 = NoiseMath.Clamp(indexPos - 1, 0, this.controlPoints.Count - 1);
            int index2 = NoiseMath.Clamp(indexPos, 0, this.controlPoints.Count - 1);
            int index3 = NoiseMath.Clamp(indexPos + 1, 0, this.controlPoints.Count - 1);

            // If some control points are missing (which occurs if the value from the
            // source module is greater than the largest input value or less than the
            // smallest input value of the control point array), get the corresponding
            // output value of the nearest control point and exit now.
            if(index1 == index2)
            {
                return this.controlPoints[index1].OutputValue;
            }

            // Compute the alpha value used for cubic interpolation.
            double input0 = this.controlPoints[index1].InputValue;
            double input1 = this.controlPoints[index2].InputValue;
            double alpha = (sourceValue - input0) / (input1 - input0);

            // Now perform the cubic interpolation given the alpha value.
            return NoiseMath.Cubic(
              this.controlPoints[index0].OutputValue,
              this.controlPoints[index1].OutputValue,
              this.controlPoints[index2].OutputValue,
              this.controlPoints[index3].OutputValue,
              alpha);
        }
    }
}
