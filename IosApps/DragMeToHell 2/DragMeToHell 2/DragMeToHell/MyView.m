//  MyView.m
//	HomeWork 4: DragMeToHell
// 	Course: CIS 651
//	Author: Ravi Nagendra and Sampath Toragaravalli Janardhan
//	SUID: 223636550
//  Created by Ravi Nagendra on 2/19/15.
//  Copyright (c) 2015 Ravi Nagendra. All rights reserved.

#import "MyView.h"

@implementation MyView

@synthesize dw, dh, row, col, x, y, inMotion, s, firstRun, xValues, yValues, previousMatchX, previousMatchY, spots, programTimer, angelImageSpot;

- (id)initWithFrame:(CGRect)frame {
    NSLog( @"initWithFrame:" );
    return self = [super initWithFrame:frame];
}

// Method to initialize the data members of the class
- (void)awakeFromNib
{
    NSLog(@"awakefromnib called");
    xValues = [[NSMutableArray alloc] initWithCapacity:20];
    yValues = [[NSMutableArray alloc] initWithCapacity:20];
    alertViewForLost = [[UIAlertView alloc] initWithTitle:@"Game Lost"
                                                  message:@"You Lost the Game !!!" delegate:self cancelButtonTitle:@"Restart Game" otherButtonTitles:nil];
    alertViewForWon = [[UIAlertView alloc] initWithTitle:@"Victory!!!"
                                                 message:@"You Won the Game !!!" delegate:self cancelButtonTitle:@"Restart Game" otherButtonTitles:nil];
    spots = [[NSMutableArray alloc] init]; //Allocate the memory to keep track of all the UIImageView elements
    _winMessage = 0;
}

- (void)drawRect:(CGRect)rect
{
    NSLog( @"drawRect:" );
    
    CGContextRef context = UIGraphicsGetCurrentContext();  // obtain graphics context
    // CGContextScaleCTM( context, 0.5, 0.5 );  // shrink into upper left quadrant
    CGRect bounds = [self bounds];          // get view's location and size
    CGFloat w = CGRectGetWidth( bounds );   // w = width of view (in points)
    CGFloat h = CGRectGetHeight ( bounds ); // h = height of view (in points)
    int currentXCoordinate = 0;
    int currentYCoordinate = 0;
    
    dw = w/10.0f;                           // dw = width of cell (in points)
    dh = h/10.0f;                           // dh = height of cell (in points)
    
    NSLog( @"view (width,height) = (%g,%g)", w, h );
    NSLog( @"cell (width,height) = (%g,%g)", dw, dh );
    NSLog(@"first run value is %d",firstRun);
    
    // draw lines to form a 10x10 cell grid
    CGContextBeginPath( context );               // begin collecting drawing operations
    for ( int i = 1;  i < 10;  ++i )
    {
        // draw horizontal grid line
        CGContextMoveToPoint( context, 0, i*dh );
        CGContextAddLineToPoint( context, w, i*dh );
        
    }
    for ( int i = 1;  i < 10;  ++i )
    {
        // draw vertical grid line
        CGContextMoveToPoint( context, i*dw, 0 );
        CGContextAddLineToPoint( context, i*dw, h );
    }
    [[UIColor grayColor] setStroke];             // use gray as stroke color
    CGContextDrawPath( context, kCGPathStroke ); // execute collected drawing ops
    
    // establish bounding box for image
    CGPoint tl = self.inMotion ? CGPointMake( x, y ): CGPointMake( row*dw, col*dh );
    
    //For the first launch of the app, generate the random numbers to place the images of the angel
    if (firstRun == 0)
    {
        firstRun++;
        tl.x = 0;
        tl.y = 0;
        
        for (int i=0; i<20; i++)
        {
            currentXCoordinate = arc4random_uniform(10);
            currentYCoordinate = arc4random_uniform(10);
            
            //If the random number generated falls on cell 0,0 or cell 9.9 then it should be regenerated
            while ((currentXCoordinate==0 && currentYCoordinate==0) || (currentXCoordinate==9 && currentYCoordinate==9) ||
                   (currentXCoordinate==0 && currentYCoordinate==1) || (currentXCoordinate==1 && currentYCoordinate==0) ||
                   (currentXCoordinate==1 && currentYCoordinate==1) || currentXCoordinate==0)
            {
                currentXCoordinate = arc4random_uniform(10);
                currentYCoordinate = arc4random_uniform(10);
            }
            
            //If the generated random values are already present in the array then we need to generate the random numbers again
            for (int m=0;m< [xValues count]; m++)
            {
                if ( ([[xValues objectAtIndex:m]integerValue] == currentXCoordinate) &&
                    ([[yValues objectAtIndex:m]integerValue] == currentYCoordinate))
                {
                    m=0;
                    currentXCoordinate = arc4random_uniform(10);
                    currentYCoordinate = arc4random_uniform(10);
                }
            }
            
            [xValues addObject: [NSNumber numberWithInt: currentXCoordinate]];
            [yValues addObject: [NSNumber numberWithInt: currentYCoordinate]];
            
            UIImage *img;
            NSString* myNewString = [NSString stringWithFormat:@"%d", i+1];
            img = [UIImage imageNamed:myNewString];
            
            UIImageView *spot = [[UIImageView alloc] initWithImage:img];
            [spots addObject:spot]; // add the spot to the spots NSMutableArray
            [self addSubview:spot]; // add the spot to the main view
            [spot setFrame:CGRectMake([[xValues objectAtIndex:i] intValue]*dw, [[yValues objectAtIndex:i] intValue]*dh, dw, dh)];
        }
         [self addAnimationForImages];
        _winMessage = 0;
        
        programTimer = [NSTimer scheduledTimerWithTimeInterval:0.005
                                                        target:self selector:@selector(timerExpired:)
                                                      userInfo:nil repeats:YES];
    }
    
   
    previousMatchX = self.x/self.dw;
    previousMatchY = self.y/self.dh;

    NSLog(@"t1 x and y values are %f %f",tl.x,tl.y);
    
    // place appropriate image where dragging stopped
    UIImage *img;
    UIImageView* currentAngelView;
    if ( self.row == 9  &&  self.col == 9 )
    {
        img = [UIImage imageNamed:@"devil"];
        currentAngelView = [[UIImageView alloc] initWithImage:img];
    }
    else
    {
        img = [UIImage imageNamed:@"angel"];
        currentAngelView = [[UIImageView alloc] initWithImage:img];
    }
    
    [self addSubview:currentAngelView];
    
    if (angelImageSpot)
        [angelImageSpot removeFromSuperview];
    angelImageSpot = currentAngelView;
    
    [currentAngelView setFrame:CGRectMake(tl.x, tl.y, dw, dh)];
}


-(void) touchesBegan: (NSSet *) touches withEvent: (UIEvent *) event
{
    int touchRow, touchCol;
    CGPoint xy;
    
    // NSLog( @"touchesBegan:withEvent:" );
    [super touchesBegan: touches withEvent: event];
    for ( id t in touches )
    {
        xy = [t locationInView: self];
        self.x = xy.x;  self.y = xy.y;
        touchRow = self.x / self.dw;
        touchCol = self.y / self.dh;
        self.inMotion = (self.row == touchRow  &&  self.col == touchCol);
        NSLog( @"touch point (x,y) = (%g,%g)", self.x, self.y );
        NSLog( @"  falls in cell (row,col) = (%d,%d)", touchRow, touchCol );
    }
}


-(void) touchesMoved: (NSSet *) touches withEvent: (UIEvent *) event
{
    int touchRow, touchCol;
    CGPoint xy;
    
    NSLog( @"touchesMoved:withEvent:" );
    [super touchesMoved: touches withEvent: event];
    
    for ( id t in touches )
    {
        xy = [t locationInView: self];
        self.x = xy.x;  self.y = xy.y;
        touchRow = self.x / self.dw;  touchCol = self.y / self.dh;
        NSLog( @"touch point (x,y) = (%g,%g)", self.x, self.y );
        NSLog( @"  falls in cell (row,col) = (%d,%d)", touchRow, touchCol );
    }
    if ( self.inMotion )
        [self setNeedsDisplay];
}

//If the angel image is placed in the last column and last row then the game should be won
-(void) touchesEnded: (NSSet *) touches withEvent: (UIEvent *) event
{
    NSLog( @"touchesEnded:withEvent:" );
   [super touchesEnded: touches withEvent: event];
    
    if ( self.inMotion )
    {
        int touchRow = 0, touchCol = 0;
        CGPoint xy ;
        
        for ( id t in touches )
        {
            xy = [t locationInView: self];
            self.x = xy.x;  self.y = xy.y;
            touchRow = self.x / self.dw;  touchCol = self.y / self.dh;
            NSLog( @"touch point (x,y) = (%g,%g)", x, y );
            NSLog( @"  falls in cell (row,col) = (%d,%d)", touchRow, touchCol );
        }
        self.inMotion = NO;
        self.row = touchRow;  self.col = touchCol;
        
        //Display a winning message when the angel reaches last row and column
        if ( self.row == 9  &&  self.col == 9 )
        {
            _winMessage = 1;
            [self setBackgroundColor: [UIColor redColor]];
            [self pauseAnimation];
            [self performSelector:@selector(delayedErrorMessageForWon) withObject:nil afterDelay:1];
            return;
        }
        else
        {
            [self setBackgroundColor: [UIColor cyanColor]];
        }
        
        [self setNeedsDisplay];
    }
}

-(void) touchesCancelled: (NSSet *) touches withEvent: (UIEvent *) event
{
    NSLog( @"touchesCancelled:withEvent:" );
    
    [super touchesCancelled: touches withEvent: event];
}

//Alert messages for game won and game lost scenarios. We need to reset all the variable so that the game will be restarted again
- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    if (alertView == alertViewForWon)
    {
        [self setBackgroundColor: [UIColor cyanColor]]; //when the game is won, we need to update the background colour to cyan again
    }
    
    [self resetGame];
}

//Reset all the variables on reset
- (void) resetGame
{
    [xValues removeAllObjects];
    [yValues removeAllObjects];
    previousMatchX = 0;
    previousMatchY = 0;
    firstRun = 0;
    self.row = 0;
    self.col = 0;
    
    [self stopAnimation];
    [spots removeAllObjects];    
    [self setNeedsDisplay];
}


//This is required to give a pause effect for the animation
- (void) stopAnimation
{
    for (int i = (int)spots.count - 1; i >= 0; --i)
    {
        UIImageView* tmp = [spots objectAtIndex:i];
        [tmp.layer removeAllAnimations]; // stop the animation
    }
}

- (void) pauseAnimation
{
    for (int i = (int)spots.count - 1; i >= 0; --i)
    {
        UIImageView *spot = [spots objectAtIndex:i];
        [self reinitiateAnimation:spot];
    }
}

// adds a new spot at a random location
- (void)addAnimationForImages
{
    for (int i=0; i<20; i++)
    {
        UIImageView* tmp = [spots objectAtIndex:i];
        NSNumber* tempnumber = [xValues objectAtIndex:i];
        long double finalnumber = [tempnumber doubleValue];
        
        
        [UIView animateWithDuration: 15.0
                              delay: 2
                            options: UIViewAnimationOptionBeginFromCurrentState | UIViewAnimationOptionTransitionNone
                         animations: ^{tmp.center = CGPointMake(finalnumber*dw+dw/2, -dh/2);}
                         completion: ^(BOOL fin) { [self finishedAnimation:@"disappeared" finished:fin context:[spots objectAtIndex:i]]; }];
    }
}

// method is automatically called when an animation ends
- (void)finishedAnimation:(NSString *)animationId finished:(BOOL)finished
                  context:(UIView *)context
{
    NSLog(@"Animation finished");
    NSLog(@"animation id is %@",animationId);
    
    if ([animationId isEqualToString:@"disappeared"] && _winMessage==0)
    {
        [self pauseAnimation];
        [self performSelector:@selector(delayedErrorMessageForLost) withObject:nil afterDelay:1];
    }
}

- (void) delayedErrorMessageForLost
{
    if (![alertViewForLost isVisible])
        [alertViewForLost show];
}

- (void) delayedErrorMessageForWon
{
    if (![alertViewForWon isVisible])
        [alertViewForWon show];
}

//check whether the angel coordinate clashes with other images
- (void)timerExpired:(NSTimer *)theTimer
{
    for( UIImageView *view in spots)
    {
        if (CGRectIntersectsRect([[angelImageSpot.layer presentationLayer] frame] , [[view.layer presentationLayer] frame]))
        {
           [theTimer invalidate];
           [self pauseAnimation];
        }
    }
}

//When the angel image clashes then remove the current animation and restart the shrink animation
- (void) reinitiateAnimation:(UIImageView *)spot
{
    CGRect frame = [[spot.layer presentationLayer] frame]; // get the frame
    [spot.layer removeAllAnimations]; // stop the animation
    spot.frame = frame; // move the spot to where the old animation ended
        // give the spot time to redraw by delaying the end animation
    [self performSelector:@selector(beginShrinkAnimation:) withObject:spot
               afterDelay:0.01];
}

//Give a shrink animation effect on winning or losing
- (void)beginShrinkAnimation:(UIImageView *)spot
{
    CGRect frame = spot.frame; // get the current frame
    frame.origin.x += frame.size.width / 2; // set x to the center
    frame.origin.y += frame.size.height / 2; // set y to the center
    frame.size.width = 0; // set the width to 0
    frame.size.height = 0; // set the height to 0
    
    [UIView animateWithDuration: 2
                          delay: 0.0
                        options: UIViewAnimationOptionBeginFromCurrentState
                     animations: ^{ [spot setFrame:frame]; [spot setAlpha:0.0]; }
                     completion: ^(BOOL fin) { [self finishedAnimation:@"shrinkanimation" finished:fin context:spot]; } ];
    
} // end method beginSpotEndAnimation:

@end
