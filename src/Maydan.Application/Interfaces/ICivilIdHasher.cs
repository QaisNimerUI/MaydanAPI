namespace Maydan.Application.Interfaces;

// Computes the deterministic blind-index value stored in Worker.CivilIdHash, used for
// uniqueness/lookup since Worker.CivilId itself is encrypted non-deterministically.
public interface ICivilIdHasher
{
    string ComputeHash(string civilId);
}
