namespace Maydan.Application.Interfaces;

// Computes the deterministic blind-index value stored in Worker.CivilIdHash, used for
// uniqueness/lookup. CivilId itself is currently PLAINTEXT (encryption via ISecretProtector is
// planned but not yet implemented — see Worker.CivilId's own comment); this hasher exists
// regardless because even once that's wired up, encrypted ciphertext can't be queried/compared
// directly, so this blind index remains the only way to look a worker up by civil ID.
public interface ICivilIdHasher
{
    string ComputeHash(string civilId);
}
