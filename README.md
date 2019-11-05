# B2Sync
A KeePass plugin that facilitates synchronization of databases with Backblaze B2 buckets.

# Disclaimer
I am in no way affiliated with or endorsed by [Backblaze](https://www.backblaze.com/) or [KeePass Password Safe](https://keepass.info/).
This is a plugin I made for my own personal use that I decided to share.

# Setup
Obviously, the use of this plugin will require a Backblaze account with [Backblaze B2](https://www.backblaze.com/b2/cloud-storage.html) enabled.

Once you have a Backblaze account and you have signed in, you will see a menu along the left of the screen looking something like this:

![A screenshot of the side menu of a Backblaze account, with the 'App Keys' menu option highlighted in yellow](/TutFiles/Step1.png "Click the 'App Keys' option")

Click the 'App Keys' option.



Next, you should see a big blue button for creating a new Application Key. Click it:

![A screenshot of a section of the 'App Keys' page, with a big blue button labelled 'Add a New Application Key'](/TutFiles/Step2.png "Click the blue button")



In the modal that pops up when you click the button, you should have the option to name your key and choose what bucket to apply it to:

![A screenshot of the modal that pops up when the button is clicked, with an option to name the key and choose it's applicable bucket, among other options](/TutFiles/Step3.png "Name the key whatever you want, but probably KeePass related")

* You can name the key whatever you want, but it would probably be wise to name it something related to KeePass.
* Make sure to change the bucket the key has permissions for to a specific bucket - **do not leave it on *All***.
* You can leave the rest of the options unchanged from their defaults.

When you're done setting up the key, click the 'Create New Key' button.



Next you should see a new blue box appear - something like the following:

![A blue box with the text 'Success! Your new application key has been created. It will only appear here once.' along the top, containing the information about the new application key.](/TutFiles/Step4.png "Make sure to copy the key ID and the application key, and put them in a notepad or something for now.")

I would suggest you open Notepad or something similar to copy the keyID and applicationKey to for now.



Now, take another look at the side menu and click on the 'Buckets' tab:

![A screenshot of the side menu of a Backblaze account, with the 'Buckets' menu option highlighted in yellow](/TutFiles/Step5.png "Click the 'Buckets' option, but make sure you have saved the application key somewhere for now.")


