using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// The class representing a Perlin Worm used to generate caves.
/// </summary>
public class CaveWorm
{
    /// <summary>
    /// Class representing one segment of a cave worm that contains a list of points/values.
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// The position of this Segment's center node in world coordinate system.
        /// </summary>
        public Vector3Int WorldPos { get; private set; }
        /// <summary>
        /// The radius of this cave segment.
        /// </summary>
        public int Radius { get; private set; }
        /// <summary>
        /// List of all points contained in this segment.
        /// </summary>
        public List<Vector3Int> Points { get; private set; } = new List<Vector3Int>();

        /// <summary>
        /// Creates a new segment with the given position in world coordinate system as its center node.
        /// </summary>
        /// <param name="worldPos">The position of the center node of this segment in world coordinate system.</param>
        /// <param name="radius">The radius of this segment.</param>
        public Segment(Vector3Int worldPos, int radius)
        {
            this.WorldPos = worldPos;
            this.Radius = radius;
        }
    }

    /// <summary>
    /// The location of this cave worm's head.
    /// </summary>
    public Vector3Int HeadPos { get; private set; }
    /// <summary>
    /// List of all the segments contained in this cave worm.
    /// </summary>
    public List<Segment> Segments { get; private set; } = new List<Segment>();

    /// <summary>
    /// Creates a new Cave Worm with the head at the given position and radius as given.
    /// Immediately runs the Generate Segments method and fills its list of segments with points.
    /// </summary>
    /// <param name="headPos">The position in world coordinate system representing the start of the worm.</param>
    public CaveWorm(Vector3Int headPos)
    {
        this.HeadPos = headPos;
        this.GenerateSegments();
    }

    /// <summary>
    /// Generates all segments for this worm. It works by looping through the maximum number of possible segments,
    /// breaking out if the next segment is more than the max distance in MaxWormChunkDistance. For each segment it 
    /// samples 3 different locations of the noise generator and uses that as a normalized vector3 representing
    /// the direction between the current segment and the next segment, it then places the next segment in that direction
    /// at radius amount of positions away from the current segment.
    /// </summary>
    public void GenerateSegments()
    {
        int dirOffset = -1000;
        this.Segments.Add(new Segment(this.HeadPos, (GameManager.Instance.MinimumCaveWormRadius + GameManager.Instance.MaximumCaveWormRadius) / 2));
        for(int currentSegment = 0; currentSegment < GameManager.Instance.MaxWormSegments - 1; currentSegment++)
        {
            float dirX = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Segments[currentSegment].WorldPos.x + (dirOffset * 1), this.Segments[currentSegment].WorldPos.y + (dirOffset * 1), this.Segments[currentSegment].WorldPos.z + (dirOffset * 1));
            float dirY = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Segments[currentSegment].WorldPos.x + (dirOffset * 2), this.Segments[currentSegment].WorldPos.y + (dirOffset * 2), this.Segments[currentSegment].WorldPos.z + (dirOffset * 2));
            float dirZ = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Segments[currentSegment].WorldPos.x + (dirOffset * 3), this.Segments[currentSegment].WorldPos.y + (dirOffset * 3), this.Segments[currentSegment].WorldPos.z + (dirOffset * 3));
            Vector3 newWormDir = new Vector3(dirX, dirY, dirZ).normalized;
            int radius = Mathf.RoundToInt(((dirX + dirY + dirZ) / 3f).Remap(-1, 1, GameManager.Instance.MinimumCaveWormRadius, GameManager.Instance.MaximumCaveWormRadius));
            Vector3Int newSegmentPos = (this.Segments[currentSegment].WorldPos + (newWormDir * radius)).RoundToInt();
            if(Vector2.Distance(this.Segments[0].WorldPos.RemoveY(), newSegmentPos.RemoveY()) < GameManager.Instance.MaxWormChunkDistance * GameManager.Instance.ChunkSize)
            {
                this.Segments.Add(new Segment(newSegmentPos, radius));
            }
            else
            {
                break;
            }
        }
        foreach(Segment segment in this.Segments)
        {
            for(int x = segment.WorldPos.x - segment.Radius; x < segment.WorldPos.x + segment.Radius; x++)
            {
                for(int y = segment.WorldPos.y - segment.Radius; y < segment.WorldPos.y + segment.Radius; y++)
                {
                    for(int z = segment.WorldPos.z - segment.Radius; z < segment.WorldPos.z + segment.Radius; z++)
                    {
                        Vector3Int nextWorldPos = new Vector3Int(x, y, z);
                        float distance = Vector3Int.Distance(segment.WorldPos, nextWorldPos);
                        if(distance <= segment.Radius)
                        {
                            segment.Points.Add(nextWorldPos);
                        }
                    }
                }
            }
        }
    }
}
