SuperHashingPasswordGenerator
=============================

An application that helps you generate different hash passwords for different websites.

This application will NOT record your secret
============================================
You can check the source by yourself

Why should I use it?
--------------------
Using different passwords for different websites is more secure.

How should I use it?
--------------------
1. Select a secret (password) and memorize it
2. For each website, add something unique (such as the domain name) as your salt
3. Generate a password that is used only for this website! (You can take the first 16 digits of the hashing result)
*  It is strongly recommended that you use two or more secrets for different categories of websitse

Initially this application uses MD5 to hash your secret. More options will be added later.
