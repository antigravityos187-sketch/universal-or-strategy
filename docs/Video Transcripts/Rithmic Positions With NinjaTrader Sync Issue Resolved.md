Rithmic Positions With NinjaTrader | Sync Issue Resolved
https://www.youtube.com/watch?v=H7VMIOPnf2E
0:01
All right, let's jump right in. Today
0:03
we're going to talk about the rhythmic
0:04
connection with Ninja Trader. So, if
0:08
you've been around prop firm trading,
0:10
trading multiple accounts for any amount
0:13
of time, you I'm sure have heard of or
0:17
dealt with the position syncing issues
0:20
between Rhythmic and Ninja Trader. This
0:24
problem has been around for a long time.
0:27
Probably as long as the rhythmic
0:30
connection has been active on Ninja
0:32
Trader with prop firm trading on
0:35
multiple accounts. Nobody's been able to
0:38
solve it. Nobody has a solution. It's
0:41
all over Discord servers. It's all over
0:43
the Ninja Trader support forum in
0:47
various threads.
0:49
I'm excited to tell you I have a
0:52
solution. We're going to dive into the
0:53
details right now. By the way, what
Trade Copier Progress
0:56
you're seeing in the background is my
0:59
trade copier. I'm selling hundreds of
1:02
these each month. It's been quite the
1:04
journey building a team to support all
1:07
the customers. And man, it's just fun
1:10
honestly working with customers to solve
1:13
challenges that arise when copying
1:15
trades across multiple accounts. As you
1:18
guys probably have experienced, no trade
1:20
copier solution is perfect, but I'm
1:22
working very hard toward the best trade
1:26
copier solution. Coming up, we're going
1:29
to cover practical examples of 23
1:33
accounts, rhythmic accounts, trading
1:36
across these accounts. We're going to
1:37
see the position syncing issue happen,
1:40
understand it, and see the solution for
1:44
resolving it. Second, I'm going to cover
1:47
the settings in this very quickly. And
1:50
then third, I'm going to discuss the
1:52
differences between rhythmic and trade
1:54
of eight and why I think that rhythmic
1:56
is actually
1:58
still the preferred connection type. So,
Rithmic Trade Examples
2:01
let's look at the screen now. In the
2:03
bottom left corner, I've got rhythmic
2:05
open. We can see the positions across
2:07
all 20 of my accounts. In here, I've got
2:11
20 accounts and three bulletinox
2:14
accounts, all rhythmic. So, what I'm
2:17
going to do is I'm going to take a trade
2:18
and I'm going to show you guys again.
2:21
We're copying these trades across all
2:24
the accounts.
2:25
I've got on executions mode.
2:29
Okay, we're short all the accounts. You
2:31
can see a slight delay, but we have all
2:33
23 accounts short. Now, when I hit
2:35
close,
2:37
five accounts remain. Five accounts have
2:40
a lingering position. So watch quickly
2:42
what happened. Did you see the
2:45
countdown? Let's do it again. Let's hit
2:48
buy market this time.
2:51
Working on the front end here. The front
2:52
end is slow sinking too. But now when we
2:55
exit the position, this is where we have
2:57
a solution. We can see in the bottom
2:58
left corner all the rhythmic accounts
3:00
are flat. We can see Ninja Trader has
3:04
four lingering accounts. These positions
3:08
are automatically refreshed by the
3:10
window. Let's take another trade.
3:13
Sell market. You can see all 20 rhythmic
3:18
accounts are short. Here we have a
3:20
lingering delay. Now they're all showing
3:23
short. Let's go ahead and fill the
3:27
target.
3:30
And again, rhythmic is flat. four
3:33
lingering positions in Ninja Trader.
3:37
The software automatically
3:40
refreshes the position. You can even see
3:42
this at the bottom. Rhythmic positions
3:44
for MNQ have been refreshed
3:47
automatically across four accounts. Your
3:49
positions in this window or in the
3:51
control center positions tab are now
3:54
synced with Rhythmic Trader Pro. So,
3:57
here's the positions tab that I'm
4:00
mentioning. But even this is incorrect.
4:02
Ninja Trader gets lingering positions.
4:06
So, let's do another one. Let's switch
4:08
it to orders mode. Let's buy limit here.
4:13
I'll go ahead and show you. Orders mode,
4:15
by the way, is excellent. It's going to
4:18
keep everything in sync. So, we can have
4:21
targets, stops. You can see the follower
4:24
count down here. Let's go ahead and move
4:26
the target and see if we can get filled.
4:29
And let's pay attention. We have
4:34
positions across all
4:37
these accounts.
4:41
Positions are now closed but lingering
4:44
in rhythmic is three accounts. See the
4:46
button
4:48
button shows three. The countdown
4:50
completes and it refreshes the
4:53
positions. This honestly guys, this
4:55
doesn't get old because I'm so proud of
4:58
what I've accomplished here. Let's get
5:00
in another position. Move the stop. Move
5:02
the target. Again, follower count stay
5:04
in sync. Notice we've got all 20
5:07
accounts in a position down here in the
5:11
Rderrader Pro window.
5:15
Just keep moving it down. I want to let
5:16
the market trade through
5:20
if possible. And there you have it. A
5:23
rare moment when all the positions
5:25
actually appear closed in Ninja Trader,
5:28
matching what you see in Rderrader Pro.
Deep Dive Into The Issue
5:39
All positions are flat. Rhythmic has two
5:41
lingering positions indicated by the
5:43
button and
5:46
it refreshes the positions
5:48
automatically. So
5:51
guys, what happens here is that a lot of
5:54
trade copers, let me try and explain
5:55
this is that if Ninja Trader is showing
5:58
a long position, let's say it's showing
6:00
a one contract long position remaining
6:03
or worse a two or three contract, right?
6:06
And you go to hit close, it's going to
6:08
submit, in the case of a long, it's
6:11
going to submit a cell to close that
6:14
out. Well, what happens when in rhythmic
6:18
at the rhythmic server, we're actually
6:20
flat.
6:22
Now, you're going to be in the opposite
6:24
position going short. I'm sure you guys
6:26
have seen this many times. So, again,
6:29
this is what's resolved with the work
6:31
that I've done in this window.
6:35
Brief interruption. I've changed into my
6:37
favorite fit, the plain black tea, to
6:39
ask you guys to like this video and
6:42
leave a comment. When you do this, it
6:45
helps YouTube know to prioritize the
6:48
content and push the video out to more
6:50
users. Plus, if you know, the rhythmic
6:54
issue has been plagging traders for a
6:56
long time. So, let's celebrate this.
7:00
Leave a comment. I'm going to read every
7:02
single comment. And when we leave
7:04
comments, it's going to get the video
7:06
out in front of more people. Thanks, you
7:09
guys. All right, let's take another
7:11
trade. I'm just going to go ahead and
7:12
hit sell market. We can see that all
7:15
rhythmic positions are filled. We've got
7:19
positions showing on Ninja Trader and
7:21
Rderrader Pro in the bottom left. Let's
7:24
go ahead and close the trade.
7:28
All right. In this case, you can see
7:30
right there, we've got two
7:32
accounts with lingering positions,
7:35
and they're automatically refreshed by
7:37
the window.
Deep Dive Into The Solution
7:39
All right. Quickly, how does this work?
7:40
First of all, it's detecting a flat
7:43
position on the master account. When
7:46
this happens, it's going to look for any
7:49
lingering positions on follower
7:51
accounts.
7:53
Also, it's detecting when the number of
7:57
positions has decreased. So, in this
7:59
case, in these examples, we have 23
8:02
positions on the NASDAQ. When that
8:04
number decreases from 21 to any number,
8:08
it's also going to check. Why is this
8:11
important? Because the master account
8:14
can also have a lingering position.
8:18
Meaning when you get out of the trade,
8:20
the master account itself can also be
8:22
left in a position, at least according
8:25
to Ninja Trader, when in reality in
8:27
rhythmic, it's already flat.
8:30
So again, very detailed, but I hope
8:33
you'll just rewatch this and take it in
8:35
because it's very important to
8:36
understand what's going on. Let's
8:39
quickly go through the settings. So
8:42
you've got this rhythmic positions
8:44
section right here. Most of this is
8:46
cosmetic, but I'm just going to cover
8:48
some of what's going on. Detection time
8:51
in seconds. Again, this is from the
8:53
moment the master account goes flat or
8:56
from the moment the number of positions
8:58
is decreasing. Either one of those
9:01
actions indicates you're trying to close
9:03
out all your positions across rhythmic
9:06
accounts. So again, this detects
9:08
rhythmic accounts. So after that moment
9:11
happens, after 2.2 seconds or however
9:15
this is configured, it's going to check
9:18
all rhythmic accounts specifically.
9:20
Again, it doesn't check other types of
9:22
accounts, just rhythmic accounts. It's
9:25
going to detect if there is an o
9:28
appearing to be an open position, right?
9:30
Be clear. Appearing to be an open
9:32
position in Ninja Trader, which more
9:34
than likely in most cases this is a
9:36
false position. Then if you have refresh
9:39
automatically checked after a certain
9:42
number of seconds here set to five, it's
9:45
going to automatically refresh those
9:47
positions across the accounts with
9:51
lingering positions. So I hope this is
9:54
clear. Again, this is just cosmetic
9:56
stuff with rhythmic and the button
9:59
color. You can also separate the button
10:01
here. I personally like
10:04
uh the button being combined with the
10:06
flatten all button. I think it's
10:08
excellent. This way you're not trying to
10:10
hit flatten all. Although I will remind
10:12
you the flatten all button is programmed
10:14
to not reverse positions. So a lot of
10:18
copers, a lot of other software, it will
10:22
completely reverse the position because
10:24
again it thinks that you're long. Ninja
10:27
Trader believes you're long. So, it's
10:29
going to submit a sell to close that,
10:32
which then puts you in a short position.
10:35
What a mess. Anyways, so I hope that is
10:39
helpful again in kind of covering the
10:41
settings and understanding what this
10:43
window is doing. All right, let's talk
Rithmic Vs. Tradovate
10:46
about trade of verse rhythmic
10:49
connections. I'm going to pull up here
10:52
what I have from Apex Trader Funding.
10:55
And this is the important info right
10:58
here. So, pause the video and read it.
11:00
But you are limited with Trade of Eate
11:04
to 5,000 unique user initiated actions
11:10
per user per rolling 60 minutes. So
11:13
again, they say placing a new order,
11:15
modifying an existing order, cancelling
11:17
an existing order are all considered
11:20
unique actions. So let's think about
11:23
this quickly. For someone who's using
11:24
trailing stops, for someone who places
11:27
and cancels orders often in my head,
11:30
quickly take 5,000 divided by 20. What
11:35
is that? 250. Am I right? 250 actions if
11:39
you're trading 20 accounts. And then the
11:41
API is going to quit working. So a lot
11:44
of times what happens is if you're using
11:46
trailing stops, bam, bam, bam, bam, bam,
11:49
your 250 commands are quickly declining.
11:54
So this is something that I want to
11:56
solve actually in another update is that
11:58
I want to keep track of these commands
12:00
in the software so that we can warn you
12:03
when you're about to run out of
12:06
commands, right? Because nothing's worse
12:08
than trading using the trade of eight
12:10
connection when all of a sudden you
12:13
can't manage the accounts anymore from
12:16
Ninja Trader. Horrible, right? Again,
12:18
that that kind of thing will never
12:21
happen with Rhythmic. This is why I
12:23
prefer Rhythmic over Trade of Especially
12:26
with what I've done in solving the
12:30
problems right here in this window.
12:33
Right? So, what I've already shown you
12:34
guys, we now know, we can be confident
12:37
that positions are synced between
12:40
Rhythmic and Ninja Trader. And this
12:43
window is handling it automatically. And
Conclusion
12:46
I'm going to be doing more videos more
12:48
frequently. I want to show you guys some
12:50
things here in these other windows. I've
12:52
got 73 accounts. Been doing some testing
12:55
lately with Apex
12:58
um Rhythmic, Apex Trade of Again, my
13:02
Trade of Eate accounts are currently
13:03
underneath the business are
13:05
disconnected, so they're showing red
13:06
dots. But this
13:10
newest version of the Trade Copier is
13:12
running so fast. Again, 73 accounts, no
13:15
problems. I'm going to be showing you
13:17
guys that in an upcoming video. Um
13:21
yeah, so exciting. it. In conclusion, I
13:24
just want to say thank you so much. You
13:26
made it all the way through the video.
13:28
I'm going to ask you to like and
13:30
comment. Especially if you're one of my
13:32
customers, I'm going to read every
13:34
single comment. If you're, you know, you
13:38
love the copier, I want to hear it. If
13:40
you have issues, I want to hear it. If
13:41
you have questions, we're going to be
13:43
looking at the questions. But yeah, like
13:46
I want to interact with you guys. I want
13:48
to see you guys lighten up the comments.
13:50
when you light up the comments, it's
13:51
going to help this content get out to
13:53
more people. Also, subscribe because I
13:56
am planning on publishing more content
13:58
more frequently coming up. And so, if
14:01
you're subscribed, you're going to be
14:02
notified right away and be in on that.
14:06
So, yeah, I just want to say thanks
14:08
again, guys. We'll see you on the next
14:12
video.