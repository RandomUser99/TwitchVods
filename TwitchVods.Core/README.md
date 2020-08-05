# TwitchVods
A console app which pulls all of the past broadcast data from the Twitch.tv API for specified channels and creates webpages from the data. 

It creates pages such as this: https://tvods.se/v/lirik.html

The app is built using .Net CORE 3.1.

You will need to specify your own settings in a settings.json file. An example one is included in the project.

You will also need to register your app (this) with Twitch to get a Client-ID and Secret to specify it in the settings.json file. 

Read about Authentication here:

https://dev.twitch.tv/docs/authentication​