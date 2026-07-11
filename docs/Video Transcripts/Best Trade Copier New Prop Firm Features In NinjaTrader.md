Best Trade Copier - New Prop Firm Features In NinjaTrader
https://www.youtube.com/watch?v=klrWtrUP7y4&t=1s




Chapter 1: Introduction & History
0:00All right, in the background is my trade copier built for the Ninja Trader desktop platform. And today in this
0:099 secondsvideo, I'm going to cover the latest features that I've designed to make it easier than ever to manage, configure,
0:1717 secondsand keep track of prop firm accounts all within one place. Let's get into the
0:2525 secondsdetails. Okay, as I scroll through my accounts, the first thing I want you to notice is the new account status flags.
Chapter 2: Account Status Flags
0:3434 secondsRight here, you have gold flags indicating that an account is funded or in other words, eligible for some sort
0:4343 secondsof a payout. And then you have gray flags which indicate that an account is in the
0:5050 secondstesting or evaluation phase. So this makes it easy to quickly see the importance of each account. Also new is
Chapter 3: Drawdown Type Column
1:001 minutethe draw down column. This column is going to let you see the type of draw down for each of your prop firm accounts. So whether it's a trailing
1:081 minute, 8 secondsdraw down, an endofday drawd down, which we have two take-profit trader
1:151 minute, 15 secondsevaluation accounts that were endofday drawdown. And then the third option is a
1:221 minute, 22 secondsstatic drawdown. So this column very important to make sure that you're
1:281 minute, 28 secondstracking along with the rules of the prop firm. And this is the most exciting
Chapter 4: Automated Prop Firm Detection
1:351 minute, 35 secondsfeature is the configuration window. So let's get started reviewing this. If you click edit next to the properform
1:431 minute, 43 secondsaccount, it's going to load a window showing all of the settings. Now these
1:501 minute, 50 secondssettings are loaded automatically from our server connected to a
1:571 minute, 57 secondsspreadsheet file. So we can continuously update this file for changes among all
2:062 minutes, 6 secondsprop firms which is super cool. So for example this account with Apex all of these green dots indicate that this has
2:162 minutes, 16 secondsbeen automatically detected. So the for this evaluation account the amount of profit profit to goal the max draw down
2:252 minutes, 25 seconds2500 what type of draw down is it trailing end of day or static and is the account
2:332 minutes, 33 secondsfunded or not. All of this is automatically configured based on detecting one the
2:412 minutes, 41 secondsname of the account and two the size of the account. These are filled in automatically. Okay, I'm back in the
2:492 minutes, 49 secondssettings and just want to show you one more thing. Proof that we are managing a list of prop firms on our server right
2:582 minutes, 58 secondshere. So, you can see that we currently have 22 prop firms with all of the settings for all of their types of
3:063 minutes, 6 secondsaccounts in our list. Again, you will see this updated right here as we update the list of prop firms on our server.
Chapter 5: Prop Firm Configuration Window
3:173 minutes, 17 secondsAll right, let's say that for a specific account, it has not been configured with the correct settings and there need to
3:253 minutes, 25 secondsbe some adjustments. No problem. You can easily do that. So, let's say that the profit here is actually 4,000.
3:333 minutes, 33 secondsYou just change this to 4,000. Let's say that the draw down is actually end of day. Change it to end of day. Now, if
3:413 minutes, 41 secondsyou take a look here, you're going to see that these dots are now gray. That indicates that the setting has now been customized
3:493 minutes, 49 secondsbased on what you're saying is the correct settings as the user. So, if I click okay on this account, we're going
3:563 minutes, 56 secondsto see this reflected. We now have from funded has changed. The draw down type has changed to end of day.
4:054 minutes, 5 secondsAnd if we go back and edit this, we can see that these are the settings that we configured for this account. Now, just to be clear, you can also reset this.
4:134 minutes, 13 secondsSo, if I hit reset, it's going to reset it back to what has been detected as the
4:214 minutes, 21 secondscorrect settings or like the automatically configured settings for that prop firm. Great. Now, let's take a
Chapter 6: Funded Account Settings
4:284 minutes, 28 secondslook at a funded account. So with funded accounts, there's one key difference and
4:344 minutes, 34 secondslooking at one of my Bulanox accounts, the difference is profit to goal. So profit to goal is not automatically
4:434 minutes, 43 secondsconfigured by our software. Reason being, your plan for requesting a payout
4:514 minutes, 51 secondsis going to be different. Everyone's plan is going to be different. There's going to be rules and differences in what your goal is before you're able to
5:005 minutesrequest a payout. So, for example, you can just set your goal in here. So, let's say that your goal is $3600.
5:095 minutes, 9 secondsYou just configure it right here. Click okay. And now it will measure how far you are in this column from funded. how
5:195 minutes, 19 secondsfar you are away from being able to request your payout. Now, scrolling down, I'll show you guys my funded
5:275 minutes, 27 secondsaccounts with Apex Trader funding. So, here I've already configured five of these accounts for this example. So,
5:345 minutes, 34 secondsI've configured them to $4,800 in a profit goal. So, I'll just go
5:425 minutes, 42 secondsthrough and quickly do the rest of these. I can copy this. Just click edit, paste. Okay.
5:515 minutes, 51 secondsAnd I won't do all these. I'll just do a few to show you. But the point is you configure for funded accounts. You're
5:585 minutes, 58 secondsconfiguring your own goal. And again, like Apex calls it the safety net, right? You need to trade these
6:056 minutes, 5 secondsparticular accounts above 52,600 and then further before you can even
6:126 minutes, 12 secondsrequest a payout. So that's why I'm trading these to 54,800
6:186 minutes, 18 secondsto be able to then request a $2,000 payout from each of the accounts. So I hope that's helpful. But yes, funded
6:276 minutes, 27 secondsaccounts in summary, you're configuring your own profit goal. The rest of the settings automatically detected as
6:356 minutes, 35 secondsindicated by these green dots. And here you can see the box checked indicating that the account is funded. One more
6:436 minutes, 43 secondsthing related to funded accounts. So we're here in the indicator settings. If we scroll down, we're going to see a
6:506 minutes, 50 secondssetting right here called performance accounts default goal. So this is the amount in dollars that you can set. And
6:586 minutes, 58 secondswhen you set it, it's going to override or it's going to set a default goal for all of your funded accounts. So if we
7:067 minutes, 6 secondsset this to 8,000, click okay. Now we can scroll through and see all the funded accounts. So here's Bouanox,
7:157 minutes, 15 secondshere's the Korean prop firm, the Apex accounts, and the take-profit accounts all have a goal now, right? So the ones
7:247 minutes, 24 secondsthat we configured earlier have the goal that we configured. But if we were to clear this out
7:317 minutes, 31 secondsand click okay, it now has a default that we just set of 8,000.
7:387 minutes, 38 secondsSo again, the goal here, 8,000, we're already about $2,000 up. So that allows you to set a default goal to be applied
7:487 minutes, 48 secondsacross all funded accounts. And then as with the other settings, you can override that in here directly. So I
7:577 minutes, 57 secondswant to pause real quick and say thank you to my customers. Every single video that I publish, you guys are in the comments lighting it up. And I hope this
8:068 minutes, 6 secondstime will be no different. We need your likes and your comments. I say this every time, but it helps YouTube know to
8:148 minutes, 14 secondsprioritize this so that we can reach more people with the software. So, I'll see you in the comments. I'm going to
8:238 minutes, 23 secondsread every single one and subscribe. I want you guys to know coming up, I'm going to be publishing content more frequently, including, as you can see
8:328 minutes, 32 secondshere, approaching payouts. This is my ultimate goal to give you guys proof of payouts. I'm actually using the software
8:418 minutes, 41 secondsmyself and this helps me understand it and continue to make it better. Thanks you guys. Next, let's take a look at
Chapter 7: Simulated Prop Firm Accounts
8:488 minutes, 48 secondssome simulated accounts. And this is big because so many of you have asked to configure a simulated account as if it's a proper account. And now you can do it.
9:009 minutesSo clicking over to the simulated accounts tab. I already have two accounts configured down here at the
9:079 minutes, 7 secondsbottom, but let's go ahead and do one together. So, we'll do SIM 105. We'll start by clicking here in the actions
9:149 minutes, 14 secondscolumn. Pop up the window. We can see that this account, it's a strange situation because it's at this price.
9:229 minutes, 22 secondsBut let's go ahead and consider this account started at 90,000. Let's say the
9:289 minutes, 28 secondsprofit to goal is 5,000 with a draw down of 2,000. And we'll leave it static. So,
9:389 minutes, 38 secondsI want you to pay attention closely here to what happens with these settings. A $5,000 goal, a $2,000 loss. It's a static loss.
9:499 minutes, 49 secondsAll right. When I click okay, the auto liquidate value is 88,000. Why
9:559 minutes, 55 secondsis that? Because being static, the point where the account will be
10:0210 minutes, 2 secondsclosed is the original balance of 90,000 minus 2,000 equals 88,000.
10:1110 minutes, 11 secondsCool. So, let's edit this account again.
10:1410 minutes, 14 secondsLet's now make this a trailing draw down and click okay. Now, we can see that we
10:2210 minutes, 22 secondshave a trailing draw down. So, this account's going to behave exactly as if it was a prop firm account with a trailing draw down of $2,000.
10:3310 minutes, 33 secondsNow, if I wanted to make this a little bit more difficult, I could change my peak balance to 396
10:4210 minutes, 42 secondsand change. Right. And when I click okay, immediately this turns yellow and it's reflected, meaning that our peak balance is in the 93,000s.
10:5410 minutes, 54 secondsminus $2,000 trail gives us only $1,200 and change of room left before the
11:0211 minutes, 2 secondsaccount is technically closed. So, this is just super cool and a big step forward being able to configure these
11:1111 minutes, 11 secondsaccounts for or to act as if they are a prop firm account. So again looking at
11:1811 minutes, 18 secondsone more sim 107 starting balance 100k peak balance we'll
11:2411 minutes, 24 secondsmake it 100k and 350 we'll say the goal here is 5,000 it
11:3211 minutes, 32 secondsreally doesn't matter max draw down 3000 end of day trail right so let's talk
11:4011 minutes, 40 secondsabout end of day trail what that means so what that means is hold up I'm going to interrupt myself because there's no
Chapter 8: Documentation
11:4711 minutes, 47 secondsneed to explain all of that in this video, but I do have a place where you can learn and read and understand. So, I
11:5511 minutes, 55 secondswant to make sure you know in these configuration windows, there's a link right here. When you click this link,
12:0412 minutes, 4 secondsit's going to pull up the website where we've created a whole page dedicated to
12:1112 minutes, 11 secondsunderstanding all of the settings and configurations for prop firm accounts. So, starting at the top, the account size, how is it
12:2012 minutes, 20 secondsdetermined, when is it detected and saved? The peak balance, same thing.
12:2712 minutes, 27 secondsSometimes you're going to have to initially set this up when you load the window. Um, and then all these fields
12:3412 minutes, 34 secondsdown here that are detected from our server. Again, explaining the green dot, the gray dot, we've covered it all down
12:4212 minutes, 42 secondsto the draw down type right here. So, make sure you utilize this guide. We're going to be continually updating it
12:4912 minutes, 49 secondsbased on your feedback. We want this to be easy for you guys. We're doing our best to make it that way. So, make sure you just reach out if you have
12:5812 minutes, 58 secondsquestions, but don't forget to utilize that resource.
Chapter 9: Cloud Based Trade Copiers
13:0213 minutes, 2 secondsAll right, I want to pause and quickly discuss cloud-based trade copier solutions. So, again, I know there's a lot of competition and I'm working
13:1113 minutes, 11 secondsaround the clock to develop the best solution right here built into the local Ninja Trader platform. So, with trades
13:1813 minutes, 18 secondsbeing executed locally, you're going to have quicker execution than trades going into a server in the cloud, then to the
13:2613 minutes, 26 secondsbroker. That's for one. But also, with my software, it's a onetime purchase, lifetime license. So, you
13:3413 minutes, 34 secondspurchase this software, I'm with you for as long as you're in this game of trading. It's a one-time purchase,
13:4113 minutes, 41 secondsunlimited updates, new features, etc. no monthly subscription, no ongoing fees.
13:5013 minutes, 50 secondsAgain, I'm trying to make this as affordable and as accessible to everyone. And so, that's just what I
13:5913 minutes, 59 secondswanted to say. I wanted to add that in the middle here. Let's keep moving. So, yeah, these accounts here are with LifeUp Trading, a Korean prop firm. But
14:0814 minutes, 8 secondsagain, just to reiterate, most of you aren't going to have to adjust these settings. These settings are directly
14:1514 minutes, 15 secondslinked to a spreadsheet on our server which we manage and update on a weekly I would say almost on a daily basis. We're updating this with
14:2414 minutes, 24 secondsthe latest settings for every account size and type across every prop firm. So most of you won't have to update this
14:3214 minutes, 32 secondswindow. But again, if there is a prop from account that is not yet detected by
14:3914 minutes, 39 secondsour software or is detected with an incorrect value, that's the idea, right?
14:4414 minutes, 44 secondsYou begin to edit these values and again the dot turns gray indicating that it has been changed and if you need to reset it, you just
14:5314 minutes, 53 secondshit reset. So yeah, that's just a reminder. Most of you won't have to touch it, but it's there if you need to
15:0015 minutesconfigure this because we want all the accounts in this window to be perfectly accurate for you guys. That's the goal.
Chapter 10: Daily Goal & Daily Loss
15:0715 minutes, 7 secondsSo, more things that I've updated in the software is the graphics. So, now the rows are highlighted when daily goal,
15:1715 minutes, 17 secondsdaily loss, or funded status is hit. So again, this makes it more visual and
15:2315 minutes, 23 secondseasier to see what's going on with all of your accounts. And just as a
15:3015 minutes, 30 secondsreminder, when you take a trade on these accounts, I'll just get into a trade as
15:3715 minutes, 37 secondsan example here. You will see that the accounts that have hit daily goal or daily loss or funded status are ignored
15:4715 minutes, 47 secondsfrom the trade. Right? This is by design. If these accounts are finished for the day, we don't want them to act as follower accounts anymore. So even
15:5515 minutes, 55 secondsthough these are configured as follower accounts, they're no longer included in a trade from the master account SIM 101.
16:0416 minutes, 4 secondsSo don't get confused by these values.
16:0716 minutes, 7 secondsThese are just configured to give us some accounts here at daily goal and daily loss. But I just want to remind you how to configure these values.
16:1616 minutes, 16 secondsAgain, you can reset these columns right here. So, if I click this twice, we'll go ahead and reset that. To configure a
16:2416 minutes, 24 secondsdaily goal, you just click in the cell and scroll your mouse wheel up and down.
16:3016 minutes, 30 secondsSo, we'll do 800 here. Again, here you just click scroll up and down. Loss of 500 to enable daily goal and daily loss.
16:4116 minutes, 41 secondsyou check the box and it's going to automatically do the same value as you go down the row. So, if I want to
16:4816 minutes, 48 secondsconfigure all these accounts, I just continue clicking through and checking the boxes. And now we're all set on all
16:5616 minutes, 56 secondsthese accounts. Daily goal 800, daily loss 500. One more way to configure these accounts is in the window. So if I
17:0517 minutes, 5 secondsclick edit, I can adjust this to any setting I want such as 850 orgative -450.
17:1517 minutes, 15 secondsSo we'll see that in this window. If these scrolling values are not accurate enough, you can adjust them further in
17:2417 minutes, 24 secondsthe window right here. In the same way that you need to check the box to activate daily goal and daily loss, you
17:3217 minutes, 32 secondsalso need to check the box to activate funded status. So by checking this box in next to the from funded column,
17:4117 minutes, 41 secondsthat's what's going to enable the software to liquidate your accounts when funded status is achieved. Same way over
17:4817 minutes, 48 secondshere by checking these boxes, it activates your accounts to be liquidated when daily goal or daily loss is hit.
Chapter 11: Exit Shield | Stop Loss Protection
17:5717 minutes, 57 secondsOne more feature, one more feature worth noting when it comes to prop firm accounts and risk management is exit
18:0418 minutes, 4 secondsshield. This feature is off by default, but once it's turned on, you will get an additional button down here. I'll turn it off and we'll get into a long trade.
18:1718 minutes, 17 secondsI'm just going to place a limit order and we'll take it up and fill it. You can see that we are in the
18:2618 minutes, 26 secondstrade. Now I'm able to move my stop loss wherever I please.
18:3418 minutes, 34 secondsYou guys see this? All right. So, let's close the trade. Let's go ahead and turn exit shield on and get into another trade.
18:4318 minutes, 43 secondsWith exit shield on, it prevents a stop-loss move away from the market. So, you can see that as I attempt to move my
18:5118 minutes, 51 secondsstop-loss, it snaps it back into place, restricting me from risking more than my original stop. So, my original stop in
19:0019 minutesthis trade was 30 points. I'm unable to move this and increase my risk, which
19:0719 minutes, 7 secondsagain prevents a lot of chaos and mistakes. Many of us are guilty of this,
19:1419 minutes, 14 secondsright? We're guilty of widening our stop loss when it's clearly against our plan.
19:1819 minutes, 18 secondsWe have the original stop. We should not ever be allowed to increase it. And so, this prevents that action automatically.
Chapter 12: Enhanced Chart Trader
19:2819 minutes, 28 secondsOkay, we're almost finished with this update. I wanted to show you guys quickly my chart trader tools. You can
19:3619 minutes, 36 secondsgo watch more videos on this. In fact, I've included them in the description.
19:4219 minutes, 42 secondsBut here you have the basic chart trader tools, things like a break even button, the ability to attach orders to
19:5019 minutes, 50 secondsindicator plots. You can see here I'm able to with one click attach this order at the 50 EMA. So I hope you guys will
20:0020 minutescheck this out even in the essential chart trader tools. Things like being able to preview your trade right here.
20:1020 minutes, 10 secondsSo I can click and preview my target and my stop and even adjust the target and
20:1720 minutes, 17 secondsstop before the trade is filled. So, I've put a ton of time into this and while the majority of my time is in the
20:2520 minutes, 25 secondstrade copier right now, the chart trader is still a big part of what I've created. And really over a thousand
20:3520 minutes, 35 secondsusers on the chart trader alone. So, I hope you guys will go check out some videos and see if there's anything in
20:4120 minutes, 41 secondshere that will help you. So, we're back to the main screen with all um Crop Firm accounts. And again, I just want to remind you that I know you have options.
Chapter 13: Conclusion
20:5120 minutes, 51 secondsAnd so, I want you to look in the description right now because I've created a trade copier comparison chart.
20:5820 minutes, 58 secondsI've also made it easy for you in the description to find a link where you can read more about this trade copier that I've created and make the purchase.
21:1021 minutes, 10 secondsLastly, I just want to give you a special thanks for making it all the way through this video, man. Thank you so
21:1721 minutes, 17 secondsmuch for giving me the time and the attention. I really appreciate it. I know your time is valuable and so I hope
21:2521 minutes, 25 secondsthis software and this content has helped you see clearly how you can manage all of your accounts easily copy
21:3421 minutes, 34 secondstrades across all of your accounts and do it at one price for life. That's what
21:4221 minutes, 42 secondsI'm trying to do here. I want to continue creating value, updating the software with new features, and I'm
21:5021 minutes, 50 secondspumped up right now because truly getting a lot of momentum in developing the software, just kind of based on my
21:5821 minutes, 58 secondscustomer feedback, based on my own experience with the software, as you can see. So, I hope you'll hit me in the
22:0522 minutes, 5 secondscomments. Let me know what you think and expect another video within the next few
22:1222 minutes, 12 secondsweeks or month as I'm going to dive deep into the Trade of Trading View platform
22:1922 minutes, 19 secondsand how the software detects orders and copies trades across accounts again with ease. That's the goal, right?
22:3222 minutes, 32 secondsI want you guys to be focused on making good trades, not having to be concerned about what are my follower accounts
22:4022 minutes, 40 secondsdoing. So, thanks for watching. I'll see you guys in the next video.

