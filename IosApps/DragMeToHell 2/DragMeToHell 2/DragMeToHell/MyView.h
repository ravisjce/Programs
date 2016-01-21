//  MyView.h
//	HomeWork 4: DragMeToHell
// 	Course: CIS 651
//	Author: Ravi Nagendra and Sampath Toragaravalli Janardhan
//	SUID: 223636550
//  Created by Ravi Nagendra on 2/19/15.
//  Copyright (c) 2015 Ravi Nagendra. All rights reserved.

#import <UIKit/UIKit.h>

@interface MyView : UIView <UIAlertViewDelegate>
{
    UIAlertView *alertViewForLost; //An alert box for the lost scenario
    UIAlertView *alertViewForWon;  //An alert box for the game winning scenario
}

@property (nonatomic, assign) CGFloat dw, dh;  // width and height of cell
@property (nonatomic, assign) CGFloat x, y;    // touch point coordinates
@property (nonatomic, assign) int row, col;    // selected cell in cell grid
@property (nonatomic, assign) BOOL inMotion;   // YES iff in process of dragging
@property (nonatomic, strong) NSString *s;     // item to drag around grid
@property (nonatomic, assign) int firstRun;    // Variable to check whether the app is launched for the first time
@property (nonatomic, strong) NSMutableArray* xValues; //Array in which the x coordinate values of the cells where the angel images are stored
@property (nonatomic, strong) NSMutableArray* yValues; //Array in which the y coordinate values of the cells where the angel images are stored
@property (nonatomic, assign) int previousMatchX; // variable to keep track of the x-coordinate of previously matched angel
@property (nonatomic, assign) int previousMatchY; // variable to keep track of the y-coordinate of previously matched angel
@property (nonatomic, strong) NSMutableArray* spots; //Array in which all the UIImageView items are stored
@property (nonatomic, weak) NSTimer* programTimer; //Timer to check whether the angel co-ordinates clashes with other iamges
@property (nonatomic, strong) UIImageView *angelImageSpot; //UIImageview of the angel image
@property (nonatomic, assign) int winMessage; //Checks whether the alert message is for the win or loss cases

- (void) reinitiateAnimation:(UIImageView *)spot; //method to start the animation in place when a game is won or lost
- (void)timerExpired:(NSTimer*)theTimer; //Method called when the timer expires
- (void) stopAnimation; //Method which is used to stop the animation
- (void) pauseAnimation; //Method which is used to give a pause effect for the animation
- (void) delayedErrorMessageForLost; //Method which is called when the game is lost
- (void) delayedErrorMessageForWon; //Method which is called when the game is won

@end
