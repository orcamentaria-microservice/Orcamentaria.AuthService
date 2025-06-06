using FluentValidation.Results;
using Orcamentaria.AuthService.Application.Validators;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using System.Security.Cryptography;

namespace Orcamentaria.AuthService.Application.Services
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public string Encript(string password)
        {
            try
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    byte[] salt = new byte[SaltSize];
                    rng.GetBytes(salt);

                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                    {
                        byte[] hash = pbkdf2.GetBytes(KeySize);
                        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public ValidationResult ValidatePattern(string password)
        {
            try
            {
                return new PasswordValidator().ValidatePattern(password);
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public bool PasswordIsValid(string password, string passwordEncript)
        {
            try
            {
                var parts = passwordEncript.Split(':');
                if (parts.Length != 2)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] hash = Convert.FromBase64String(parts[1]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hashToCompare = pbkdf2.GetBytes(KeySize);
                    return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
                }
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }
    }
}
