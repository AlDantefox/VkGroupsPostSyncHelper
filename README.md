# VkGroupsPostSyncHelper
Simple NET Core service for share social networks posts from VK.
Using Vk.Net c# library and Telegram.Bot for send message to channels.

Local posts history saving in SQLite database.
There are two ways for post message (include in appsettings.json)
1) Post only latest (last 24 hours) messages from VK (you choose time interval in Crontab format for try count)
2) Post in differen interval old posts. You can use setting for set "border" from most oldest post will be choose to send

Application support only message and image(photo) content from VK, but other imports (video, articles planned to add)

