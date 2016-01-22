## Website Scraper

This is a simple console application which takes in a website URL, and creates a folder for that website in the same directory as the executable.
The program will then begin to download the website, inspecting the pages of media type `text/html` for other pages and media, which will get in turn downloaded.
All downloaded media will maintain the file structure of the remote website, so additional folders will be created when required.
The downloads will be async, and general progress will be output to the console.