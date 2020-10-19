using SharpNoise.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNoise.Builders
{
    /// <summary>
    /// Builds a planar noise map.
    /// </summary>
    /// <remarks>
    /// This class builds a noise map by filling it with coherent-noise values
    /// generated from the surface of a plane.
    ///
    /// This class describes these input values using (x, z) coordinates.
    /// Their y coordinates are always 0.0.
    ///
    /// The application must provide the lower and upper x coordinate bounds
    /// of the noise map, in units, and the lower and upper z coordinate
    /// bounds of the noise map, in units.
    ///
    /// To make a tileable noise map with no seams at the edges, modify the
    /// <see cref="EnableSeamless"/> property.
    /// </remarks>
    public class PlaneNoiseMapBuilder : NoiseMapBuilder
    {
        /// <summary>
        /// Enables or disables seamless tiling.
        /// </summary>
        /// <remarks>
        /// Enabling seamless tiling builds a noise map with no seams at the
        /// edges.  This allows the noise map to be tileable.
        /// </remarks>
        public bool EnableSeamless { get; set; }

        /// <summary>
        /// Gets the lower x boundary of the planar noise map.
        /// </summary>
        public double LowerXBound { get; private set; }

        /// <summary>
        /// Gets the upper x boundary of the planar noise map.
        /// </summary>
        public double UpperXBound { get; private set; }

        /// <summary>
        /// Gets the lower z boundary of the planar noise map.
        /// </summary>
        public double LowerZBound { get; private set; }

        /// <summary>
        /// Gets the upper z boundary of the planar noise map.
        /// </summary>
        public double UpperZBound { get; private set; }

        /// <summary>
        /// Sets the boundaries of the planar noise map.
        /// </summary>
        /// <param name="lowerXBound">The lower x boundary of the noise map, in
        /// units.</param>
        /// <param name="upperXBound">The upper x boundary of the noise map, in
        /// units.</param>
        /// <param name="lowerZBound">The lower z boundary of the noise map, in
        /// units.</param>
        /// <param name="upperZBound">The upper z boundary of the noise map, in
        /// units.</param>
        public void SetBounds(double lowerXBound, double upperXBound, double lowerZBound, double upperZBound)
        {
            if(lowerXBound >= upperXBound ||
                lowerZBound >= upperZBound)
            {
                throw new ArgumentException("Lower bounds must be less than upper bounds.");
            }

            this.LowerXBound = lowerXBound;
            this.UpperXBound = upperXBound;
            this.LowerZBound = lowerZBound;
            this.UpperZBound = upperZBound;
        }

        protected override void PrepareBuild()
        {
            if(this.LowerXBound >= this.UpperXBound ||
                this.LowerZBound >= this.UpperZBound ||
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
            Plane planeModel = new Plane(this.SourceModule);

            double xExtent = this.UpperXBound - this.LowerXBound;
            double zExtent = this.UpperZBound - this.LowerZBound;
            double xDelta = xExtent / this.destWidth;
            double zDelta = zExtent / this.destHeight;

            ParallelOptions po = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            Parallel.For(0, this.destHeight, po, z =>
            {
                double zCur = this.LowerZBound + z * zDelta;

                int x;
                double xCur;

                for(x = 0, xCur = this.LowerXBound; x < this.destWidth; x++, xCur += xDelta)
                {
                    float finalValue;
                    if(!this.EnableSeamless)
                    {
                        finalValue = (float)planeModel.GetValue(xCur, zCur);
                    }
                    else
                    {
                        double swValue = planeModel.GetValue(xCur, zCur);
                        double seValue = planeModel.GetValue(xCur + xExtent, zCur);
                        double nwValue = planeModel.GetValue(xCur, zCur + zExtent);
                        double neValue = planeModel.GetValue(xCur + xExtent, zCur + zExtent);
                        double xBlend = 1.0 - ((xCur - this.LowerXBound) / xExtent);
                        double zBlend = 1.0 - ((zCur - this.LowerZBound) / zExtent);
                        double z0 = NoiseMath.Linear(swValue, seValue, xBlend);
                        double z1 = NoiseMath.Linear(nwValue, neValue, xBlend);
                        finalValue = (float)NoiseMath.Linear(z0, z1, zBlend);
                    }
                    this.DestNoiseMap[x, z] = finalValue;
                }
            });
        }
    }
}
