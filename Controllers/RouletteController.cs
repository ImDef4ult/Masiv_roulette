using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace source_code_api.Controllers
{
    
    [Route("api/roulette/")]
    [ApiController]
    public class RouletteController : ControllerBase
    {
        #region MYSQL SETUP 
        // Install-Package MySql.Data -Version 8.0.21
        private MySqlConnection GetConnection()
        {
            //change your connection string below
            // you can also put your connection string in app settings file and fetch from there.
            return new MySqlConnection("Server=192.168.1.112;Port=3306;Database=roulettedb;Uid=root;Pwd=adminsgd;");
        }
        #endregion

        #region API CALL ON START UP OF API JUST FOR INFO

        [HttpGet]
        [Route("start")]
        public IActionResult FirstApi()
        {
            return Ok(@"This APi is working fine, use swagger, postman or insomnia client to try it. SWAGGER is at URL : 'http://localhost:5000/swagger/index.html' ");
        }

        #endregion

        #region CLIENT LIST (Only Admin)

        // [Authorize] // use autherization if you want
        [HttpGet]
        [Route("Client_list")]
        public async Task<IActionResult> ClientList()
        {
            // response holder for error message
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                //int _accessUserId = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);
                    //_accessUserId = Convert.ToInt32(token.Payload["id"]);

                    // CLIENT LIST
                    if (_accessRole == "Admin")
                    {

                        List<Client> _employeeList = new List<Client>();

                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd = new MySqlCommand("select id_client,name,password,balance from client", conn);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _employeeList.Add(new Client()
                                    {
                                        Id = Convert.ToInt32(reader["id_client"]),
                                        Name = reader["name"].ToString(),
                                        Password = reader["password"].ToString(),
                                        Balance = Convert.ToInt32(reader["balance"]),
                                    });
                                }
                            }
                        }

                        var result = await Task.Run(() => _employeeList);

                        return new OkObjectResult(result);
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Admin role can add new users";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }
                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }
        }

        #endregion 


        // GENERACION DE TOKENS DE SESION
        #region GENERATE TOKEN FOR AUTHENTICATION ADMIN

        [HttpPost]
        [AllowAnonymous]
        [Route("generate/token_admin")]
        public async Task<IActionResult> GenerateToken([FromBody]  LoginData _loginData)
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    if (conn != null && conn.State == ConnectionState.Closed)
                    {
                        conn.Open();

                    }
                    MySqlCommand cmd = new MySqlCommand("select id_admin from admin_users where username='" + _loginData.Username + "' and password='" + _loginData.Password + "'", conn);
                    int _userId = Convert.ToInt32(cmd.ExecuteScalar());
                    if (_userId > 0 )
                    {
                        // Response Example, See Response.cs file or modify it as you like.

                        var TokenResponse = new TokenResponse
                        {
                            // GenerateJwtToken() is used to generate token if username & password matches
                            AccessToken = GenerateJwtToken(_userId, "Admin"),
                            HelpInfo = "Anonymous function is used to generate tokens."
                        };

                        // call response data as task

                        var result = await Task.Run(() => TokenResponse);

                        // Return Respose 

                        return new OkObjectResult(result);
                    }
                    else
                    {
                        //invalid login
                        // response holder for error message
                        var _responseHolder = new ResponseHolder();
                        _responseHolder.Info = "You are not a valid user so token generation failed.";
                        var result = await Task.Run(() => _responseHolder);
                        return new UnauthorizedObjectResult(result);
                    }
                }
            }
            catch (Exception ex)
            {
                var _responseHolder = new ResponseHolder();
                _responseHolder.Info = "Error in processing.";
                var result = await Task.Run(() => _responseHolder);
                return new UnauthorizedObjectResult(result);
            }
        }

        #endregion

        #region GENERATE TOKEN FOR AUTHENTICATION Client

        [HttpPost]
        [AllowAnonymous]
        [Route("generate/token_client")]
        public async Task<IActionResult> GenerateTokenClient([FromBody] LoginData _loginData)
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    if (conn != null && conn.State == ConnectionState.Closed)
                    {
                        conn.Open();

                    }
                    MySqlCommand cmd = new MySqlCommand("select id_client from client where name='" + _loginData.Username + "' and password='" + _loginData.Password + "'", conn);
                    int _userId = Convert.ToInt32(cmd.ExecuteScalar());
                    if (_userId > 0)
                    {
                        // Response Example, See Response.cs file or modify it as you like.

                        var TokenResponse = new TokenResponse
                        {
                            // GenerateJwtToken() is used to generate token if username & password matches
                            AccessToken = GenerateJwtToken(_userId, "Client"),
                            HelpInfo = "Anonymous function is used to generate tokens."
                        };

                        // call response data as task

                        var result = await Task.Run(() => TokenResponse);

                        // Return Respose 

                        return new OkObjectResult(result);
                    }
                    else
                    {
                        //invalid login
                        // response holder for error message
                        var _responseHolder = new ResponseHolder();
                        _responseHolder.Info = "You are not a valid user so token generation failed.";
                        var result = await Task.Run(() => _responseHolder);
                        return new UnauthorizedObjectResult(result);
                    }
                }
            }
            catch (Exception ex)
            {
                var _responseHolder = new ResponseHolder();
                _responseHolder.Info = "Error in processing.";
                var result = await Task.Run(() => _responseHolder);
                return new UnauthorizedObjectResult(result);
            }
        }

        #endregion

        // Agrear usuarios nuevos (Solo Admin puede hacerlo)
        #region ADD CLIENT

        // If someone hit this url without token than, it will give 401 code.
        // Also remember your request header should have 'access_token' name variable to retrive claim details.
        [HttpPost]
        [Authorize]
        [Route("addClient")]
        public async Task<IActionResult> AddClient([FromBody]  Client _client)
        {
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                int insertedId = 0;
                //int _accessUserId = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);
                    //_accessUserId = Convert.ToInt32(token.Payload["id"]);

                    // ADD CLIENT
                    if (_accessRole == "Admin")
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd = new MySqlCommand("insert into client(name,password,balance) values('" + _client.Name + "','" + _client.Password + "','" + _client.Balance + "');", conn);
                            insertedId = Convert.ToInt32(cmd.ExecuteNonQuery());

                        }
                        //
                        if (insertedId > 0)
                        {
                            _responseHolder.Info = "Data added successfully.";

                            var result = await Task.Run(() => _responseHolder);

                            return new OkObjectResult(result);

                        }
                        else
                        {
                            _responseHolder.Info = "Some error  occurred in Saving.";

                            var result = await Task.Run(() => _responseHolder);

                            return new BadRequestObjectResult(await Task.Run(() => _responseHolder));

                        }
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Admin role can add new users";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }

                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }

        }

        #endregion

        // Agrear ruleta nuevos (Solo Admin puede hacerlo)
        #region ADD ROULETTE

        // If someone hit this url without token than, it will give 401 code.
        // Also remember your request header should have 'access_token' name variable to retrive claim details.
        [HttpPost]
        [Authorize]
        [Route("addRoulette")]
        public async Task<IActionResult> AddRoulette([FromBody] Roulette _roulette)
        {
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                int insertedId = 0;
                //int _accessUserId = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);
                    //_accessUserId = Convert.ToInt32(token.Payload["id"]);

                    // ADD CLIENT
                    if (_accessRole == "Admin")
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd = new MySqlCommand("insert into roulette(roulette_name,roulette_state) values('" + _roulette.Name + "'," + false + ");", conn);
                            insertedId = Convert.ToInt32(cmd.ExecuteNonQuery());

                        }
                        //
                        if (insertedId > 0)
                        {
                            _responseHolder.Info = "Roulette added successfully. Roulette ID: " + insertedId;

                            var result = await Task.Run(() => _responseHolder);

                            return new OkObjectResult(result);

                        }
                        else
                        {
                            _responseHolder.Info = "Some error  occurred in Saving.";

                            var result = await Task.Run(() => _responseHolder);

                            return new BadRequestObjectResult(await Task.Run(() => _responseHolder));

                        }
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Admin role can add new users";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }

                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }

        }

        #endregion

        // Listar ruletas
        #region ROULETTE LIST (Authenticated user)

        // [Authorize] // use autherization if you want
        [HttpGet]
        [Route("Roulette_list")]
        public async Task<IActionResult> RouletteList()
        {
            // response holder for error message
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                //int _accessUserId = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);
                    //_accessUserId = Convert.ToInt32(token.Payload["id"]);

                    // CLIENT LIST
                    if (_accessRole == "Admin" || _accessRole == "Client")
                    {

                        List<Roulette> _rouletteList = new List<Roulette>();

                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd = new MySqlCommand("select id_roulette,roulette_name,roulette_state from roulette", conn);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _rouletteList.Add(new Roulette()
                                    {
                                        Id = Convert.ToInt32(reader["id_roulette"]),
                                        Name = reader["roulette_name"].ToString(),
                                        State = (bool)(reader["roulette_state"]),
                                    });
                                }
                            }
                        }

                        var result = await Task.Run(() => _rouletteList);

                        return new OkObjectResult(result);
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Admin role can add new users";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }
                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }
        }

        #endregion

        // Hacer apuesta (Int target)
        #region ADD BET INT

        // If someone hit this url without token than, it will give 401 code.
        // Also remember your request header should have 'access_token' name variable to retrive claim details.
        [HttpPost]
        [Authorize]
        [Route("addBet_Int")]
        public async Task<IActionResult> AddBET_Int ([FromBody] Bet_int _bet)
        {
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                int insertedId = 0;
                int _accessUserId = 0;
                int User_balance = 0;
                int roulette_state = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);
                    _accessUserId = Convert.ToInt32(token.Payload["id"]);

                    // ADD CLIENT
                    if (_accessRole == "Client")
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd_roulette = new MySqlCommand("SELECT roulette_state FROM roulette WHERE id_roulette = " + _bet.Id_roulette + "", conn);
                            roulette_state = Convert.ToInt32(cmd_roulette.ExecuteScalar());
                            if (roulette_state == 1)
                            {
                                MySqlCommand cmd = new MySqlCommand("select balance from client where id_client=" + _accessUserId + "", conn);
                                User_balance = Convert.ToInt32(cmd.ExecuteScalar());
                                if ((User_balance - _bet.Bet_value) >= 0)
                                {
                                    if ((_bet.Bet_target_int >=0) && (_bet.Bet_target_int <= 36))
                                    {
                                        MySqlCommand cmd_bet = new MySqlCommand("insert into bets(id_client,id_roulette,bet_value, bet_target_int, gain) values('" + _accessUserId + "','" + _bet.Id_roulette + "','" + _bet.Bet_value + "','" + _bet.Bet_target_int + "','" + 0 + "');", conn);
                                        insertedId = Convert.ToInt32(cmd_bet.ExecuteNonQuery());

                                        int FinalBalance = User_balance - _bet.Bet_value;
                                        MySqlCommand cmd_update_balance = new MySqlCommand("update  client set balance=" + FinalBalance + " where id_client=" + _accessUserId + " ;", conn);
                                        int updatedRows_balance = Convert.ToInt32(cmd_update_balance.ExecuteNonQuery());
                                    }
                                    else
                                    {
                                        _responseHolder.Info = "Bet invalid.";
                                        var result = await Task.Run(() => _responseHolder);
                                        return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                                    }
                                }
                                else
                                {
                                    _responseHolder.Info = "Not balance enought to make a bet.";
                                    var result = await Task.Run(() => _responseHolder);
                                    return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                                }
                            }
                            else
                            {
                                _responseHolder.Info = "The roulette is not available yet.";
                                var result = await Task.Run(() => _responseHolder);
                                return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                            }

                        }
                        //
                        if (insertedId > 0)
                        {
                            _responseHolder.Info = "Bet added successfully.";

                            var result = await Task.Run(() => _responseHolder);

                            return new OkObjectResult(result);

                        }
                        else
                        {
                            _responseHolder.Info = "Some error  occurred in Saving.";

                            var result = await Task.Run(() => _responseHolder);

                            return new BadRequestObjectResult(await Task.Run(() => _responseHolder));

                        }
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Client role can add new users";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }

                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }

        }

        #endregion

        // Hacer apuesta (Int target)
        #region ADD BET COLOR

        // If someone hit this url without token than, it will give 401 code.
        // Also remember your request header should have 'access_token' name variable to retrive claim details.
        [HttpPost]
        [Authorize]
        [Route("addBet_Target")]
        public async Task<IActionResult> AddBET_target([FromBody] Bet_color _bet)
        {
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                int insertedId = 0;
                int _accessUserId = 0;
                int User_balance = 0;
                int roulette_state = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);
                    _accessUserId = Convert.ToInt32(token.Payload["id"]);

                    // ADD CLIENT
                    if (_accessRole == "Client")
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd_roulette = new MySqlCommand("SELECT roulette_state FROM roulette WHERE id_roulette = " + _bet.Id_roulette + "", conn);
                            roulette_state = Convert.ToInt32(cmd_roulette.ExecuteScalar());
                            if (roulette_state == 1)
                            {
                                MySqlCommand cmd = new MySqlCommand("SELECT balance FROM client WHERE id_client=" + _accessUserId + "", conn);
                                User_balance = Convert.ToInt32(cmd.ExecuteScalar());
                                if ((User_balance - _bet.Bet_value) >= 0)
                                {
                                    if ((_bet.Bet_target_color.Equals("Rojo")) || (_bet.Bet_target_color.Equals("Negro")))
                                    {
                                        int color_value;
                                        color_value = _bet.Bet_target_color.Equals("Rojo") ? 0 : 1;

                                        MySqlCommand cmd_bet = new MySqlCommand("INSERT into bets(id_client,id_roulette,bet_value, bet_target_color) values('" + _accessUserId + "','" + _bet.Id_roulette + "','" + _bet.Bet_value + "'," + color_value + ");", conn);
                                        insertedId = Convert.ToInt32(cmd_bet.ExecuteNonQuery());

                                        int FinalBalance = User_balance - _bet.Bet_value;
                                        MySqlCommand cmd_update_balance = new MySqlCommand("UPDATE client SET balance=" + FinalBalance + " WHERE id_client=" + _accessUserId + " ;", conn);
                                        int updatedRows_balance = Convert.ToInt32(cmd_update_balance.ExecuteNonQuery());
                                    }
                                    else
                                    {
                                        _responseHolder.Info = "Bet invalid.";
                                        var result = await Task.Run(() => _responseHolder);
                                        return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                                    }
                                }
                                else
                                {
                                    _responseHolder.Info = "Not balance enought to make a bet.";
                                    var result = await Task.Run(() => _responseHolder);
                                    return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                                }
                            }
                            else
                            {
                                _responseHolder.Info = "The roulette is not available yet.";
                                var result = await Task.Run(() => _responseHolder);
                                return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                            }

                        }
                        //
                        if (insertedId > 0)
                        {
                            _responseHolder.Info = "Bet added successfully.";

                            var result = await Task.Run(() => _responseHolder);

                            return new OkObjectResult(result);

                        }
                        else
                        {
                            _responseHolder.Info = "Some error  occurred in Saving.";

                            var result = await Task.Run(() => _responseHolder);

                            return new BadRequestObjectResult(await Task.Run(() => _responseHolder));

                        }
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Client role can add new users";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }

                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }

        }

        #endregion

        // Cambiar estado de la ruleta
        #region CHANGE ROULETTE STATE

        // If someone hit this url without token than, it will give 401 code.
        // Also remember your request header should have 'access_token' name variable to retrive claim details.
        [HttpPost]
        [Authorize]
        [Route("openRoulette")]
        public async Task<IActionResult> openRoulette([FromBody] Roulette_state _rouletteState)
        {
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                int updatedRows = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);

                    if (_accessRole == "Admin")
                    {
                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd = new MySqlCommand("update  roulette set roulette_state=" + true + " where id_roulette=" + _rouletteState.Id+ " ;", conn);
                            updatedRows = Convert.ToInt32(cmd.ExecuteNonQuery());

                        }
                        //
                        if (updatedRows > 0)
                        {
                            _responseHolder.Info = "Data updated successfully.";

                            var result = await Task.Run(() => _responseHolder);

                            return new OkObjectResult(result);

                        }
                        else
                        {
                            _responseHolder.Info = "Some error  occurred in Saving.";

                            var result = await Task.Run(() => _responseHolder);

                            return new BadRequestObjectResult(await Task.Run(() => _responseHolder));

                        }
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Admin role can change the state of the roulettes.";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }

                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }

        }

        #endregion


        //Cerrar ruleta
        #region CLOSE ROULETTE 

        // If someone hit this url without token than, it will give 401 code.
        // Also remember your request header should have 'access_token' name variable to retrive claim details.
        [HttpPost]
        [Authorize]
        [Route("closeRoulette")]
        public async Task<IActionResult> closeRoulette([FromBody] Roulette_state _rouletteState)
        {
            var _responseHolder = new ResponseHolder();

            try
            {
                string _accessRole = "";
                int updatedRows_balance = 0;
                if (User.Identity.IsAuthenticated)
                {
                    // example retrive claim details from token.
                    var jwtEncodedString = await HttpContext.GetTokenAsync("access_token");
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    _accessRole = Convert.ToString(token.Payload["access_role"]);

                    if (_accessRole == "Admin")
                    {
                        Random rnd = new Random();
                        //int iWin = rnd.Next(0, 37);
                        int iWin = 25;

                        List<Winners> _winners_int = new List<Winners>();
                        List<Winners> _winners_color = new List<Winners>();
                        using (MySqlConnection conn = GetConnection())
                        {
                            if (conn != null && conn.State == ConnectionState.Closed)
                            {
                                conn.Open();

                            }
                            MySqlCommand cmd_roulette = new MySqlCommand("SELECT roulette_state FROM roulette WHERE id_roulette = " + _rouletteState.Id + "", conn);
                            int roulette_state = Convert.ToInt32(cmd_roulette.ExecuteScalar());
                            if (roulette_state == 1)
                            {
                                MySqlCommand cmd_Win_int = new MySqlCommand("SELECT id_client, bet_value, id_bet FROM bets WHERE bet_target_int = " + iWin + " and id_roulette =  " + _rouletteState.Id + "  ; ", conn);
                                using (var reader = cmd_Win_int.ExecuteReader())
                                {
                                    _responseHolder.Info = "Winner Number: " + iWin;
                                    while (reader.Read())
                                    {
                                        _winners_int.Add(new Winners()
                                        {
                                            id_client = Convert.ToInt32(reader["id_client"]),
                                            bet_value = Convert.ToInt32(reader["bet_value"]),
                                            id_bet = Convert.ToInt32(reader["id_bet"]),
                                        });
                                    }
                                }

                                int color_value = iWin % 2 == 0 ? 0 : 1;
                                MySqlCommand cmd_Win_color = new MySqlCommand("SELECT id_client, bet_value, id_bet FROM bets WHERE bet_target_color = " + color_value + " and id_roulette =  " + _rouletteState.Id + "  ; ", conn);
                                using (var reader = cmd_Win_color.ExecuteReader())
                                {
                                    _responseHolder.Info = "Winner Number: " + iWin;
                                    while (reader.Read())
                                    {
                                        _winners_color.Add(new Winners()
                                        {
                                            id_client = Convert.ToInt32(reader["id_client"]),
                                            bet_value = Convert.ToInt32(reader["bet_value"]),
                                            id_bet = Convert.ToInt32(reader["id_bet"]),
                                        });
                                    }
                                }

                                foreach (Winners user in _winners_int)
                                {
                                    MySqlCommand cmd_update = new MySqlCommand("UPDATE bets SET gain=" + user.bet_value * 5.8 + " WHERE id_client=" + user.id_client + " and id_bet =" + user.id_bet + " ;", conn);
                                    int updatedRows_bet = Convert.ToInt32(cmd_update.ExecuteNonQuery());
                                    if (updatedRows_bet > 0)
                                    {
                                        _responseHolder.Info += "- Winner client: " + user.id_client;
                                        MySqlCommand cmd_User = new MySqlCommand("SELECT balance FROM client WHERE id_client=" + user.id_client + "", conn);
                                        int User_balance = Convert.ToInt32(cmd_User.ExecuteScalar());
                                        int FinalBalance = (int)(User_balance + +(user.bet_value * 5));
                                        MySqlCommand cmd_update_balance = new MySqlCommand("UPDATE client SET balance=" + FinalBalance + " WHERE id_client=" + user.id_client + " ;", conn);
                                        updatedRows_balance = Convert.ToInt32(cmd_update_balance.ExecuteNonQuery());
                                    }
                                }

                                foreach (Winners user in _winners_color)
                                {
                                    MySqlCommand cmd_update = new MySqlCommand("UPDATE bets SET gain=" + user.bet_value * 1.8 + " WHERE id_client=" + user.id_client + " and id_bet =" + user.id_bet + " ;", conn);
                                    int updatedRows_bet = Convert.ToInt32(cmd_update.ExecuteNonQuery());
                                    if (updatedRows_bet > 0)
                                    {
                                        _responseHolder.Info += "- Winner color client: " + user.id_client;
                                        MySqlCommand cmd_User = new MySqlCommand("SELECT balance FROM client WHERE id_client=" + user.id_client + "", conn);
                                        int User_balance = Convert.ToInt32(cmd_User.ExecuteScalar());
                                        int FinalBalance = (int)(User_balance + +(user.bet_value * 5.8));
                                        MySqlCommand cmd_update_balance = new MySqlCommand("UPDATE client SET balance=" + FinalBalance + " WHERE id_client=" + user.id_client + " ;", conn);
                                        updatedRows_balance = Convert.ToInt32(cmd_update_balance.ExecuteNonQuery());
                                    }
                                }


                                MySqlCommand cmd = new MySqlCommand("UPDATE roulette SET roulette_state=" + false + " WHERE id_roulette=" + _rouletteState.Id + " ;", conn);
                                int updatedRows = Convert.ToInt32(cmd.ExecuteNonQuery());
                            }
                            else
                            {
                                _responseHolder.Info = "The roulette is not available yet.";
                                var result = await Task.Run(() => _responseHolder);
                                return new ConflictObjectResult(await Task.Run(() => _responseHolder));
                            }
                        }
                        //
                        if (updatedRows_balance > 0)
                        {
                            var result = await Task.Run(() => _responseHolder);

                            return new OkObjectResult(result);

                        }
                        else
                        {
                            _responseHolder.Info = "Some error  occurred in Saving.";

                            var result = await Task.Run(() => _responseHolder);

                            return new BadRequestObjectResult(await Task.Run(() => _responseHolder));

                        }
                    }
                    else
                    {
                        // If user is not authorized than give UnauthorizedObject Result (401) 
                        _responseHolder.Info = "Only users with Admin role can change the state of the roulettes.";
                        return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                    }

                }
                else
                {
                    // If user is not authorized than give UnauthorizedObject Result (401) 
                    _responseHolder.Info = "Invalid Data. User is not authenticated.";
                    return new UnauthorizedObjectResult(await Task.Run(() => _responseHolder));
                }
            }
            catch (Exception ex)
            {
                // If some error occured than give bad result (400) 
                // You can modify as you want
                _responseHolder.Info = "Some error  occurred in Processing.";
                return new BadRequestObjectResult(await Task.Run(() => _responseHolder));
            }

        }

        #endregion

        #region Funtion TO GENERATE JWT TOKEN
        private string GenerateJwtToken(int _id, string _role)
        {
            // Basic structure of Jwt Token
            int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UseAnySecreateKeyHereForToken")); // use same key as defined in Startup.cs file
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create the JWT security token claims so you retrive it later if you want from token.
            var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "Subject Here"),
                    new Claim(JwtRegisteredClaimNames.Jti, "jti"),
                    new Claim(JwtRegisteredClaimNames.Iat, epoch.ToString(), ClaimValueTypes.Integer64),
                     new Claim("id",_id.ToString()), // set  custom id which you want to retrive from token later
                    new Claim("access_role",_role) // set any custom details which you want to retrive from token
                    
            };

            var jwt = new JwtSecurityToken(
            issuer: "example.com",
            audience: "example.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(120), // token will expire after 120 minutes
            signingCredentials: credentials
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        #endregion
      
    }
}
