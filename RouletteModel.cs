using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace source_code_api
{
    public class Client 
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("balance")]
        public int Balance { get; set; }

    }

    public class Roulette
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("state")]
        public bool State { get; set; }

    }
    public class Roulette_state
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class Winners
    {
        [JsonPropertyName("id_client")]
        public int id_client { get; set; }

        [JsonPropertyName("id_bet")]
        public int id_bet { get; set; }

        [JsonPropertyName("bet_value")]
        public int bet_value { get; set; }
    }

    public class Bet_int
    {
        [JsonPropertyName("id_roulette")]
        public int Id_roulette { get; set; }

        [JsonPropertyName("bet_value")]
        public int Bet_value { get; set; }

        [JsonPropertyName("bet_target_int")]
        public int Bet_target_int { get; set; }

    }

    public class Bet_color
    {
        [JsonPropertyName("id_roulette")]
        public int Id_roulette { get; set; }

        [JsonPropertyName("bet_value")]
        public int Bet_value { get; set; }

        [JsonPropertyName("bet_target_color")]
        public string Bet_target_color { get; set; }

    }

    public class Bet_Color
    {
        [JsonPropertyName("id_roulette")]
        public int Id_roulette { get; set; }

        [JsonPropertyName("bet_value")]
        public int Bet_value { get; set; }

        [JsonPropertyName("bet_target_color")]
        public string Bet_target_color { get; set; }

    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("help_info")]
        public string HelpInfo { get; set; }
    }

    public class LoginData
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class ResponseHolder
    {
        [JsonPropertyName("info")]
        public string Info { get; set; }

    }
}
