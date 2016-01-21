//  DMTHAppDelegate.m
//	HomeWork 4: DragMeToHell
// 	Course: CIS 651
//	Author: Ravi Nagendra and Sampath Toragaravalli Janardhan
//	SUID: 223636550
//  Created by Ravi Nagendra on 2/19/15.
//  Copyright (c) 2015 Ravi Nagendra. All rights reserved.

#import "DMTHAppDelegate.h"
#import "DMTHViewController.h"

@implementation DMTHAppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    self.window = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
    // Override point for customization after application launch.
    self.viewController = [[DMTHViewController alloc] initWithNibName:@"DMTHViewController" bundle:nil];
    self.window.rootViewController = self.viewController;
    [self.window makeKeyAndVisible];
    return YES;
}

@end
