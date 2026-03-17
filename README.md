# Project Development Details
This project was built using the express edition of Microsoft Visual Studio 2010 C#, targeting .NET Framework 4.0. This project does not have any dependencies aside from .NET framework 4.0.

# Project Intent
The intent of this project is to condense a webpage from a series of files spread throughout a folder structure into a single .html file in order to be able to deploy the webpage (an application built using web technologies like HTML, CSS, Javascript and JSON) to any device that supports a browser that will open and display HTML files, such as mobile devices where security restrictions prevent arbitrary access of files from the user's device. 

The idea behind this design strategy is to leverage the abstraction that HTML and related technologies employ to be able to run in many different browsers and environments, reducing development time and effort needed to port code to different environments and platforms since browsers automatically handle client-specific functions like component render/layout and event handling logic. Besides faster development time, this approach has several advantages.

Firstly, applications developed using this approach will be able to seamlessly connect to the internet without requiring separate dependencies, since HTML and related technologies are built around internet connectivity. Furthermore, browsers are often built with user security as a primary factor since security weaknesses have and continue to be exploited by bad actors to attack users. Consequently, applications built around this framework may be safer for users to run than an application running within the operating system, since the browser enforces security features to protect the user. Finally, no setup is required - users can simply download the application HTML file and open it in the browser of choice. This makes it much easier for users to engage with the application and safer as well, since administrator rights typically required for application installation & setup are simply not required.

One disadvantage to building an application using this approach might be a lack of performance, owing to browsers being in a higher level of abstraction from the device hardware than an application running directly in the operating system. However, for computationally intensive applications, the best approach is probably to develop the application to run within the device operating system directly to take full advantage of the hardware.

# Licensing & Project Use Details
This project is licensed under the MIT license. If you use the code within this project, please include a link to this project's Github page and a copy of the license.
