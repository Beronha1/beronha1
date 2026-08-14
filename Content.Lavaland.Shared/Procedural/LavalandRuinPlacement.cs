// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;

namespace Content.Lavaland.Shared.Procedural;

/// <summary>
/// Pure placement helpers shared by Lavaland generation and its tests.
/// Keeping candidate generation independent from entity state makes a seed's
/// placement input reproducible and keeps exclusion rules easy to audit.
/// </summary>
public static class LavalandRuinPlacement
{
    public static List<Vector2i> GenerateCandidates(int distance, int maxDistance)
    {
        if (distance <= 0)
            throw new ArgumentOutOfRangeException(nameof(distance), "Ruin distance must be greater than zero.");

        if (maxDistance < 0)
            throw new ArgumentOutOfRangeException(nameof(maxDistance), "Maximum ruin distance cannot be negative.");

        var coords = new List<Vector2i>();
        var moveVector = new Vector2i(maxDistance, maxDistance);

        while (moveVector.Y >= -maxDistance)
        {
            while (moveVector.X > -maxDistance)
            {
                coords.Add(moveVector);
                moveVector += new Vector2i(-distance, 0);
            }

            coords.Add(moveVector);
            moveVector += new Vector2i(0, -distance);

            while (moveVector.X < maxDistance)
            {
                coords.Add(moveVector);
                moveVector += new Vector2i(distance, 0);
            }

            coords.Add(moveVector);
            moveVector += new Vector2i(0, -distance);
        }

        return coords;
    }

    public static List<Vector2i> ExcludeReserved(
        IEnumerable<Vector2i> candidates,
        IReadOnlyCollection<Box2> reserved)
    {
        return candidates
            .Where(coord => reserved.All(box => !box.Contains(coord)))
            .ToList();
    }
}
