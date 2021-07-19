# Masiv_roulette
This API was made with .NET CORE using MySql as Data Base.

## Steps to use the API:
<p>1- Recover the Data base using the .sql script in the repository</p>
<p>2- Open the .sln file and start the API</p>
<p>3- Enter in Swagger UI in: http://localhost:5000/swagger/index.html </p>
<p>4- Use  "GENERATE TOKEN FOR AUTHENTICATION ADMIN" function with user: ImDef4ult, password: 12345</p>
<p>5- Copy the token in Authorize button and write: bearer token</p>
<p>5- Create a user using "addClient" function </p>
<p>6- Create a Roulette (By default, the roulette is disabled) </p>
<p>7- Change the state of this roulette using the ID (Client only can bet if the roulette is enabled)
<p>8- Get the authetication token with this client </p>
<p>9- Bet in a specific roulette (Use "addBet_Int" function to bet a number or "addBet_Target" to bet a color)</p>
<p>10- As an Admin, close the roulette using the ID to calculate the winner.</p>
