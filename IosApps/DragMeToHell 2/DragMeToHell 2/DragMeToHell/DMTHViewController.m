//  DMTHViewController.m
//	HomeWork 4: DragMeToHell
// 	Course: CIS 651
//	Author: Ravi Nagendra and Sampath Toragaravalli Janardhan
//	SUID: 223636550
//  Created by Ravi Nagendra on 2/19/15.
//  Copyright (c) 2015 Ravi Nagendra. All rights reserved.

#import "DMTHViewController.h"

@implementation DMTHViewController

- (void)viewDidLoad
{
    NSLog( @"viewDidLoad" );
    [super viewDidLoad];
    [self.view setBackgroundColor: [UIColor cyanColor]];   
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    // Return YES for supported orientations
    return (interfaceOrientation != UIInterfaceOrientationPortraitUpsideDown);
}

@end
