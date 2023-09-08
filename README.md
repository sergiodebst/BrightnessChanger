# BrightnessChanger
Windows application to create keyboard shortcut to increase/decrease the screen brightness

## Must hook application
Sometimes there are applications that hooks the windows messages and deny the registered windows HotKey to be executed (normally fullscreen games). 
So in order to get our HotKeys executed, it is posible to specify that conflictive applications inside a file. You have to create the hook file in the same path of the application executable and call it "hook".
Edit that file as text and put there the name of the conflictive application .exe file.

If there is something written in the hook file, a "watcher" will be created traking the current active app, and when an app specified in the hook file is detected, we create a windows messages hook that will have more 
priority than the one created by the conflictive app, so when we detect that a shortcut of any of the registered hotkeys is pressed we can execute it.
