using System;

namespace SharpNoise.Modules
{
    /// <summary>
    /// Noise module that rotates the input value around the origin before
    /// returning the output value from a source module.
    /// </summary>
    /// <remarks>
    /// The <see cref="GetValue"/> method rotates the coordinates of the input value
    /// around the origin before returning the output value from the source
    /// module.  To set the rotation angles, call the <see cref="SetAngles"/> method.  To
    /// set the rotation angle around the individual x, y, or z axes,
    /// modify the <see cref="XAngle"/>, <see cref="YAngle"/> or <see cref="ZAngle"/> properties,
    /// respectively.
    ///
    /// The coordinate system of the input value is assumed to be
    /// "left-handed" (x increases to the right, y increases upward,
    /// and z increases inward.)
    ///
    /// This noise module requires one source module.
    /// </remarks>
    [Serializable]
    public class RotatePoint : Module
    {
        /// <summary>
        /// Default rotation angle for all axes
        /// </summary>
        public const double DefaultRotation = 0D;

        // 1st column: x1, x2, x3
        // 2nd column: y1, y2, y3
        // 3rd column: z1, z2, z3
        protected double[,] matrix;
        protected double xAngle, yAngle, zAngle;

        /// <summary>
        /// Gets or sets the first source module
        /// </summary>
        public Module Source0
        {
            get { return this.SourceModules[0]; }
            set { this.SourceModules[0] = value; }
        }

        /// <summary>
        /// Gets or sets the rotation angle around the x axis to apply to the
        /// input value.
        /// </summary>
        public double XAngle
        {
            get
            {
                return this.xAngle;
            }
            set
            {
                this.SetAngles(value, this.yAngle, this.zAngle);
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle around the y axis to apply to the
        /// input value.
        /// </summary>
        public double YAngle
        {
            get
            {
                return this.yAngle;
            }
            set
            {
                this.SetAngles(this.xAngle, value, this.zAngle);
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle around the z axis to apply to the
        /// input value.
        /// </summary>
        public double ZAngle
        {
            get
            {
                return this.zAngle;
            }
            set
            {
                this.SetAngles(this.xAngle, this.yAngle, value);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RotatePoint()
            : base(1)
        {
            this.matrix = new double[3, 3];
            this.SetAngles(DefaultRotation, DefaultRotation, DefaultRotation);
        }

        /// <summary>
        /// Sets the rotation angles around all three axes to apply to the
        /// input value.
        /// </summary>
        /// <param name="xAngle">The rotation angle around the x axis, in degrees.</param>
        /// <param name="yAngle">The rotation angle around the x axis, in degrees.</param>
        /// <param name="zAngle">The rotation angle around the x axis, in degrees.</param>
        public void SetAngles(double xAngle, double yAngle, double zAngle)
        {
            double xCos, yCos, zCos, xSin, ySin, zSin;
            xCos = Math.Cos(xAngle * NoiseMath.DegToRad);
            yCos = Math.Cos(yAngle * NoiseMath.DegToRad);
            zCos = Math.Cos(zAngle * NoiseMath.DegToRad);
            xSin = Math.Sin(xAngle * NoiseMath.DegToRad);
            ySin = Math.Sin(yAngle * NoiseMath.DegToRad);
            zSin = Math.Sin(zAngle * NoiseMath.DegToRad);

            this.matrix[0, 0] = ySin * xSin * zSin + yCos * zCos;
            this.matrix[1, 0] = xCos * zSin;
            this.matrix[2, 0] = ySin * zCos - yCos * xSin * zSin;
            this.matrix[0, 1] = ySin * xSin * zCos - yCos * zSin;
            this.matrix[1, 1] = xCos * zCos;
            this.matrix[2, 1] = -yCos * xSin * zCos - ySin * zSin;
            this.matrix[0, 2] = -ySin * xCos;
            this.matrix[1, 2] = xSin;
            this.matrix[2, 2] = yCos * xCos;

            this.xAngle = xAngle;
            this.yAngle = yAngle;
            this.zAngle = zAngle;
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
            double nx = (this.matrix[0, 0] * x) + (this.matrix[1, 0] * y) + (this.matrix[2, 0] * z);
            double ny = (this.matrix[0, 1] * x) + (this.matrix[1, 1] * y) + (this.matrix[2, 1] * z);
            double nz = (this.matrix[0, 2] * x) + (this.matrix[1, 2] * y) + (this.matrix[2, 2] * z);
            return this.SourceModules[0].GetValue(nx, ny, nz);
        }
    }
}
