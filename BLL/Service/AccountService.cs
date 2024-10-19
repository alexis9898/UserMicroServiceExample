using Azure.Identity;
using BLL.Models;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDataContext _context;
        private readonly IConfiguration _configuration;

        public AccountService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AppDataContext context,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> SignUp(SignUp signUp)
        {
            var user= await SaveAccount(signUp);
            if (user == null) return null;


            //send request create trainer

            return user.Id;
        }
        public async Task<AppUser> SaveAccount(SignUp signUp)
        {
            var newUser = new AppUser()
            {
                UserName = signUp.UserName,
                //PhoneNumber = signUp.Phone,
            };

            //if (signUp.Role != Role.Manager && signUp.Role != Role.DeliveryPersons)
            //   return null;

            var result = await _userManager.CreateAsync(newUser, signUp.Password);

            if (result.Succeeded)
            {
                var createdUser = await _userManager.FindByNameAsync(signUp.UserName);
                if (createdUser != null)
                {
                    //await _userManager.addtoroleasync(createduser, signup.role);
                    return createdUser;
                }
                return null;
            }
            else
                return null;
        }


        public async Task<AccountModel> LoginAsync(SignUp signUp)
        {
            var result = await _signInManager.PasswordSignInAsync(signUp.UserName, signUp.Password,false,false);

            if(!result.Succeeded)
                return null;

            var user = await _userManager.FindByNameAsync(signUp.UserName);
            if (user == null)
                return null;
            // var roles = await _userManager.GetRolesAsync(user);
            //var token = generateToken(signInModel.UserName,roles[0]);
            var id = user.Id;
            var token = generateToken(signUp.UserName,"");
            var _token = new JwtSecurityTokenHandler().WriteToken(token);
            var exp = token.ValidTo;

            return new AccountModel() { Id = id, Name = signUp.UserName, _token = _token, _tokenExpirationDate = exp };

        }

        public JwtSecurityToken generateToken(string UserName, string role)
        {
            var authClaims = new List<Claim>
            {
                //new Claim("id", "12345"),
                new Claim( ClaimTypes.Name, UserName),
                //new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSignKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudiences"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256Signature));
        
            return token;
        }
        

    }
}
