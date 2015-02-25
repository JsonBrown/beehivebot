﻿#Beehive Bot

Beehive bot is a modular IRC bot loosely based on Modbot by Jonathan Smith (Keirathi).
It was written to be an extensible baseline bot for twitch streamers that want basic IRC chatbot functionality, along with in-stream interaction.

## Supported commands:

### !addcmd *[command]* *[response]*
(ops only)
This will add a command to the bot. When any user in the channel submits the *[command]* the bot will respond using the given *[response]*

### !delcmd *[command]*
(ops only)
this will remove a *[command]* added by !addcmd

### !addquote *[quote]*
(ops only)
This will add a quote to the bot. quotes can be retrieved by !quote command

### !delquote *[quote number]*
(ops only)
This will remove a quote added by !addquote

### !quote *[quote number (optional)]*
This will retrieve a quote from the DB. If a number is specified, that quote will be retrieved, otherwise a random quote will be shown.

### !buzz
This will send a Buzz to the internal website hosted in Beehive Bot (http://localhost:8335) for use with window capture, or CLR browser source plugin


## Installation
Coming soon...