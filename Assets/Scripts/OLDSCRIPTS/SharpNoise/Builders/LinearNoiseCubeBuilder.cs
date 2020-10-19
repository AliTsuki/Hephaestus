using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNoise.Builders
{
    /// <summary>
    /// Builds a linear noise cube.
    /// </summary>
    /// <remarks>
    /// This class builds a noise cube by filling it with coherent-noise values
    /// generated linearly from the given bounds.
    ///
    /// This class describes these input values using (x, y, z) coordinates.
    ///
    /// The application must provide the lower and upper x, y and z coordinate bounds
    /// of the noise cube, in units.
    /// </remarks>
    public class LinearNoiseCubeBuilder : NoiseCubeBuilder
    {
        /// <summary>Gets the lower x boundary of the planar noise cube.</summary>
        public double LowerXBound { get; private set; }
        /// <summary>Gets the upper x boundary of the planar noise cube.</summary>
        public double UpperXBound { get; private set; }

        /// <summary>Gets the lower y boundary of the planar noise cube.</summary>
        public double LowerYBound { get; private set; }
        /// <summary>Gets the upper y boundary of the planar noise cube.</summary>
        public double UpperYBound { get; private set; }

        /// <summary>Gets the lower z boundary of the planar noise cube.</summary>
        public double LowerZBound { get; private set; }
        /// <summary>Gets the upper z boundary of the planar noise cube.</summary>
        public double UpperZBound { get; private set; }

        /// <summary>
        /// Sets the boundaries of the planar noise cube.
        /// </summary>
        /// <param name="lowerXBound">The lower x boundary of the noise cube, in units.</param>
        /// <param name="upperXBound">The upper x boundary of the noise cube, in units.</param>
        /// <param name="lowerYBound">The lower y boundary of the noise cube, in units.</param>
        /// <param name="upperYBound">The upper y boundary of the noise cube, in units.</param>
        /// <param name="lowerZBound">The lower z boundary of the noise cube, in units.</param>
        /// <param name="upperZBound">The upper z boundary of the noise cube, in units.</param>
        public void SetBounds(double lowerXBound, double upperXBound,
            double lowerYBound, double upperYBound,
            double lowerZBound, double upperZBound)
        {
            if(lowerXBound >= upperXBound || lowerYBound >= upperYBound || lowerZBound >= upperZBound)
            {
                throw new ArgumentException("Lower bounds must be less than upper bounds.");
            }

            this.LowerXBound = lowerXBound;
            this.UpperXBound = upperXBound;
            this.LowerYBound = lowerYBound;
            this.UpperYBound = upperYBound;
            this.LowerZBound = lowerZBound;
            this.UpperZBound = upperZBound;
        }

        protected override void PrepareBuild()
        {
            if(this.LowerXBound >= this.UpperXBound || this.LowerYBound >= this.UpperYBound || this.LowerZBound >= this.UpperZBound ||
                this.destWidth <= 0 || this.destHeight <= 0 || this.destDepth <= 0 ||
                this.SourceModule == null || this.DestNoiseCube == null)
            {
                throw new InvalidOperationException("Builder isn't properly set up.");
            }

            this.DestNoiseCube.SetSize(this.destWidth, this.destHeight, this.destDepth);
        }

        protected override void BuildImpl(CancellationToken cancellationToken)
        {
            double xExtent = this.UpperXBound - this.LowerXBound;
            double yExtent = this.UpperYBound - this.LowerYBound;
            double zExtent = this.UpperZBound - this.LowerZBound;
            double xDelta = xExtent / this.destWidth;
            double yDelta = yExtent / this.destHeight;
            double zDelta = zExtent / this.destDepth;

            ParallelOptions po = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            Parallel.For(0, this.destDepth, po, z =>
            {
                double zCur = this.LowerZBound + z * zDelta;

                double yCur = this.LowerYBound;
                for(int y = 0; y < this.destHeight; y++)
                {
                    double xCur = this.LowerXBound;
                    for(int x = 0; x < this.destWidth; x++)
                    {
                        float finalValue = (float)this.SourceModule.GetValue(xCur, yCur, zCur);
                        xCur += xDelta;
                        this.DestNoiseCube[x, y, z] = finalValue;
                    }
                    yCur += yDelta;
                }
                zCur += zDelta;
            });
        }
    }
}
