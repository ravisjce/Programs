   M S B A M L   `   `   `  ����  ServerExecutive     ServerExecutive.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  
MainWindow��5   	   $	��350��6   $	��700��6)   .��5      �  � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      $��40 �6   �� 5	      $��* �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� -   5         ��Name$ label1��5   @   $��28��6   $	��150��6    $��
100,6,30,6q�6,   $��Center=�6P   $��1q�5      
�������$��Center=�65   $��Enter the port number��6Y   �� 5      -   5         ��Name$ RemoteAddressTextBox1��5   <   $��24��6   $	��120��6"   $
��30,8q�6.   $��Center=�6[   $��http://localhost:��6v   �� 5      -   5        $ RemotePortTextBox1��5   2   $��23��6   $��20,8.5q�6"   $��Center=�6N   $��78��6i   $��Center=�5      $
��4000��62   5      �� 5   
   $��1��6   $ 
Horizontal=�6"   .+�5      �� -   5         ��Name$ ListenButton1��5   5   $��24��6   $��
180,8,40,8q�6!   $��Center=�6L   $��78��5      $��Listen��6   $��Center=�6F   �� 5      -   5        $ 
StopButton��5   3   $��24��6   $��1,8,30,8q�6!   $��Center=�6G   $��78��5      $
��Stop��6   6(   �� 5      -   5        $ BrowseButton��5   1   $��Browse��6   $��78��6&   $��24��6G   $	��0,8q�6S   {�.  6v   5      � 5   
   $��2��6   .+�5      �� -   5        $��listBox1��5   >   $��0��6   $��30, 20, 15, 20q�6&   $	��170��6N   �� 5      $��Server Messages��6   $
��Left=�6.   $��40,5,0,0q�6I   $	��Top=�6[   5      5      5      