using SharpNoise.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNoise.Builders
{
    /// <summary>
    /// Builds a cylindrical noise map.
    /// </summary>
    /// <remarks>
    /// This class builds a noise map by filling it with coherent-noise values
    /// generated from the surface of a cylinder.
    ///
    /// This class describes these input values using an (angle, height)
    /// coordinate system.  After generating the coherent-noise value from the
    /// input value, it then "flattens" these coordinates onto a plane so that
    /// it can write the values into a two-dimensional noise map.
    ///
    /// The cylinder model has a radius of 1.0 unit and has infinite height.
    /// The cylinder is oriented along the y axis.  Its center is at the
    /// origin.
    ///
    /// The x coordinate in the noise map represents the angle around the
    /// cylinder's y axis.  The y coordinate in the noise map represents the
    /// height above the x-z plane.
    ///
    /// The application must provide the lower and upper angle bounds of the
    /// noise map, in degrees, and the lower and upper height bounds of the
    /// noise map, in units.
    /// </remarks>
    public class CylinderNoiseMapBuilder : NoiseMapBuilder
    {
        /// <summary>
        /// Gets the lower angle boundary of the cylindrical noise map, in degrees.
        /// </summary>
        public double LowerAngleBound { get; private set; }

        /// <summary>
        /// Gets the lower height boundary of the cylindrical noise map, in units.
        /// </summary>
        public double LowerHeightBound { get; private set; }

        /// <summary>
        /// Gets the upper angle boundary of the cylindrical noise map, in degrees.
        /// </summary>
        public double UpperAngleBound { get; private set; }

        /// <summary>
        /// Gets the upper height boundary of the cylindrical noise map, in units.
        /// </summary>
        public double UpperHeightBound { get; private set; }

        /// <summary>
        /// Sets the coordinate boundaries of the noise map.
        /// </summary>
        /// <param name="lowerAngleBound">The lower angle boundary of the noise map, in degrees.</param>
        /// <param name="upperAngleBound">The upper angle boundary of the noise map, in degrees.</param>
        /// <param name="lowerHeightBound">The lower height boundary of the noise map, in units.</param>
        /// <param name="upperHeightBound">The upper height boundary of the noise map, in units.</param>
        /// <remarks>
        /// One unit is equal to the radius of the cylinder.
        /// </remarks>
        public void SetBounds(double lowerAngleBound, double upperAngleBound, double lowerHeightBound, double upperHeightBound)
        {
            if(lowerAngleBound >= upperAngleBound ||
                lowerHeightBound >= upperHeightBound)
            {
                throw new ArgumentException("Lower bounds must be less than upper bounds.");
            }

            this.LowerAngleBound = lowerAngleBound;
            this.UpperAngleBound = upperAngleBound;
            this.LowerHeightBound = lowerHeightBound;
            this.UpperHeightBound = upperHeightBound;
        }

        protected override void PrepareBuild()
        {
            if(this.LowerAngleBound >= this.UpperAngleBound ||
                this.LowerHeightBound >= this.UpperHeightBound ||
                this.destWidth <= 0 ||
                this.destHeight <= 0 ||
                this.SourceModule == null ||
                this.DestNoiseMap == null)
            {
                throw new InvalidOperationException("Builder isn't properly set up.");
            }

            this.DestNoiseMap.SetSize(this.destHeight, this.destWidth);
        }

        protected override void BuildImpl(CancellationToken cancellationToken)
        {
            Cylinder cylinderModel = new Cylinder(this.SourceModule);

            double angleExtent = this.UpperAngleBound - this.LowerAngleBound;
            double heightExtent = this.UpperHeightBound - this.LowerHeightBound;
            double xDelta = angleExtent / this.destWidth;
            double yDelta = heightExtent / this.destHeight;
            double curAngle = this.LowerAngleBound;
            double curHeight = this.LowerHeightBound;

            ParallelOptions po = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            Parallel.For(0, this.destHeight, po, y =>
            {
                for(int x = 0; x < this.destWidth; x++)
                {
                    float curValue = (float)cylinderModel.GetValue(curAngle, curHeight);
                    this.DestNoiseMap[x, y] = curValue;
                    curAngle += xDelta;
                }
                curHeight += yDelta;
            });
        }
    }
}
