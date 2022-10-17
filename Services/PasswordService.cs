using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace elefanti.video.backend.Services;
public class PasswordService {

    
    /**
     * This function hashes and salts the password input recieved by the user,
     * and returns the hash and the salt.
     *  
     **/
    public string HashPassword(string password) {

        byte[] salt = RandomNumberGenerator.GetBytes(256 / 8);

        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8)
            );


        return $"{Convert.ToBase64String(salt)}:{hash}";
    }

    /**
     * This function recieves and compares the user input to the existing password,
     * returns true if the password is correct.
     *      
     **/   
    public bool VerifyPassword(string userInput, string existingPassword) {

        var inputArray = existingPassword.Split(":");

        byte[] salt = Convert.FromBase64String(inputArray[0]);
        var existingHash = inputArray[1];

        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: userInput,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8)
            );


        return existingHash == hashed;
    }
}
