using SharpNoise.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNoise.Builders
{
    /// <summary>
    /// Builds a spherical noise map.
    /// </summary>
    /// <remarks>
    /// This class builds a noise map by filling it with coherent-noise values
    /// generated from the surface of a sphere.
    ///
    /// This class describes these input values using a (latitude, longitude)
    /// coordinate system.  After generating the coherent-noise value from the
    /// input value, it then "flattens" these coordinates onto a plane so that
    /// it can write the values into a two-dimensional noise map.
    ///
    /// The sphere model has a radius of 1.0 unit.  Its center is at the
    /// origin.
    ///
    /// The x coordinate in the noise map represents the longitude.  The y
    /// coordinate in the noise map represents the latitude.  
    ///
    /// The application must provide the southern, northern, western, and
    /// eastern bounds of the noise map, in degrees.
    /// </remarks>
    public class SphereNoiseMapBuilder : NoiseMapBuilder
    {
        /// <summary>
        /// Gets the eastern boundary of the spherical noise map.
        /// </summary>
        public double EastLonBound { get; private set; }

        /// <summary>
        /// Gets the northern boundary of the spherical noise map.
        /// </summary>
        public double NorthLatBound { get; private set; }

        /// <summary>
        /// Gets the southern boundary of the spherical noise map.
        /// </summary>
        public double WestLonBound { get; private set; }

        /// <summary>
        /// Gets the western boundary of the spherical noise map.
        /// </summary>
        public double SouthLatBound { get; private set; }

        /// <summary>
        /// Sets the coordinate boundaries of the noise map.
        /// </summary>
        /// <param name="southLatBound">The southern boundary of the noise map, in
        /// degrees.</param>
        /// <param name="northLatBound">The northern boundary of the noise map, in
        /// degrees.</param>
        /// <param name="westLonBound">The western boundary of the noise map, in
        /// degrees.</param>
        /// <param name="eastLonBound">The eastern boundary of the noise map, in
        /// degrees.</param>
        public void SetBounds(double southLatBound, double northLatBound, double westLonBound, double eastLonBound)
        {
            if(southLatBound >= northLatBound ||
                westLonBound >= eastLonBound)
            {
                throw new ArgumentException("Lower bounds must be less than upper bounds.");
            }

            this.SouthLatBound = southLatBound;
            this.NorthLatBound = northLatBound;
            this.WestLonBound = westLonBound;
            this.EastLonBound = eastLonBound;
        }

        protected override void PrepareBuild()
        {
            if(this.EastLonBound <= this.WestLonBound ||
                this.NorthLatBound <= this.SouthLatBound ||
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
            Sphere sphereModel = new Sphere(this.SourceModule);

            double lonExtent = this.EastLonBound - this.WestLonBound;
            double latExtent = this.NorthLatBound - this.SouthLatBound;
            double xDelta = lonExtent / this.destWidth;
            double yDelta = latExtent / this.destHeight;

            ParallelOptions po = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            Parallel.For(0, this.destHeight, po, y =>
            {
                double curLat = this.SouthLatBound + y * yDelta;

                int x;
                double curLon = this.WestLonBound;

                for(x = 0, curLon = this.WestLonBound; x < this.destWidth; x++, curLon += xDelta)
                {
                    float curValue = (float)sphereModel.GetValue(curLat, curLon);
                    this.DestNoiseMap[x, y] = curValue;
                }
            });
        }
    }
}
