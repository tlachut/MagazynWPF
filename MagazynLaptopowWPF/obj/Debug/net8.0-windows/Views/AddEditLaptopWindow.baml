   M S B A M L   `   `   `  ����M  IMagazynLaptopowWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:MagazynLaptopowWPF.ViewsMagazynLaptopowWPF.Views  5   	   M+clr-namespace:MagazynLaptopowWPF.ViewModelsMagazynLaptopowWPF.ViewModels  5   	   2    ,MagazynLaptopowWPF.Views.AddEditLaptopWindow   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   8local&clr-namespace:MagazynLaptopowWPF.Views     5   	   :vm+clr-namespace:MagazynLaptopowWPF.ViewModels     5   	   T PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3f6mah,http://metro.mahapps.com/winfx/xaml/controls  5   	   c _MahApps.Metro.IconPacks.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137ag cMahApps.Metro.IconPacks.Material, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137a?	iconPacks-http://metro.mahapps.com/winfx/xaml/iconpacks   5	   	   $	��400��5   '   $	��500��64     � WindowStartupLocation$  CenterOwner=�5   	    � ShowInTaskbar$ False��5   	      MahApps.Brushes.Accent(  "MahApps.Metro.Controls.MetroWindow   	GlowBrush# �   5   	   $��1q�5   	   #���   6    � Title 5   	   �� *WindowTitle+ � 	Resources 5        ValidationTextBlockStyle �� 
TargetType �� Property �� Value  MahApps.Styles.TextBlock �� BasedOn��%T  &	       '�� �     7  �� 5   
   " ��61   .�5      ��  � ��
 �  ��6+   �� 5       � ��$ 11(�6)   �� 5       � ��$ 5,2,0,5q�6'   5      �� 5   
   " ��6   8   .�5      ��  � ��$ Center=�62   �� 5       � ��$ 0,0,10,5q�6'   �� 5       � ��$ SemiBold"�6+   5      5      .��5      � $��20q�6   	 � RowDefinitions	 5    
   �� 5!      $
��Auto �6   �� 5"      $
��Auto �6   �� 5#      $
��Auto �6   �� 5$      $
��Auto �6   �� 5%      $
��Auto �6   �� 5&      $��* �6   �� 5'      $
��Auto �6   
 � ColumnDefinitions
 5)   
   �� 5*      $
��Auto �6   �� 5+      $��* �6   .+�5/   
   �� $��0��6   $��0��6!   $��2��61   $��18(�51      $
��Bold"�6"   $��0,0,0,15q�64   ��50      �� *WindowTitle+51   4   �� 54   
   $��1��6   $��0��6!   $��Marka:��61   �� 55   
   $��1��6   $��1��6"   .+�56      �� *  $MahApps.Metro.Controls.TextBoxHelper   	Watermark$ Wpisz markę...��57      $��0,0,0,2q�6C   ��56      �� *Marka+ �� UpdateSourceTrigger$ PropertyChanged=� �� ValidatesOnDataErrors$
 True��57   C   �� 58      #��[ 61   ��6   �� *	[Marka]+61   59      �� 5<   
   $��2��6   $��0��6!   $��Model:��61   �� 5=   
   $��2��6   $��1��6"   .+�5>      �� $ Wpisz model...��5?      $��0,0,0,2q�6C   ��5>      �� *Model+$ PropertyChanged=�$
 True��5?   C   �� 5@      #��[ 61   ��6   �� *	[Model]+61   5A      �� 5D   
   $��3��6   $��0��6!   $��System Operacyjny:��61   �� 5E   
   $��3��6   $��1��6   $. (Wpisz system operacyjny (opcjonalnie)...��5F      $��0,0,0,10q�6Y   ��5E   /   �� *SystemOperacyjny+$ PropertyChanged=�5F   Y   �� 5I   
   $��4��6   $��0��6!   $��Rozmiar Ekranu (cale):��61   �� 5J   
   $��4��6   $��1��6"   .+�5K      *  $MahApps.Metro.Controls.NumericUpDown     Minimum$ 0Q�5L         Maximum$	 100Q�6+      Interval$	 0.1Q�69      StringFormat$ F1��6H   $��0,0,0,2q�5M         Value 5K       �� *RozmiarEkranu+$ PropertyChanged=�$
 True��5M      �� 5N      #��[ 69   ��6   �� *[RozmiarEkranu]+69   5O      �� 5R   
   $��5��6   $��0��6!   $��Ilość (szt.):��61   �� 5S   
   $��5��6   $��1��6"   .+�5T        $ 0Q�5U      $ 10000Q�6+   $ 1Q�6;   $��0,0,0,2q�5V       5T       �� *Ilosc+$ PropertyChanged=�$
 True��5V      �� 5W      #��[ 61   ��6   �� *	[Ilosc]+61   5X      A� 5[   
   $��6��6   $��0��6#   $��2��63    A� Rows$ 1��6G    A� Columns$ 2��6P   $��Right=�6\   $��0,20,0,0q�6x   .+�5\      �� -   5         ��Name$ 
SaveButton��5\       �� 	IsDefault$
 True��6B   $��Zapisz��5]      $	��100��6&   $��0,0,10,0q�65    ' #MahApps.Styles.Button.Square.Accent#��[ 5_      {�5^      �� *	IsValid+��5`      �� 5a      .R�5b      ��  �� Orientation$ 
Horizontal=�6%   .+�5c      .  (MahApps.Metro.IconPacks.PackIconMaterial     Kind$ ContentSave=�69   $��0,0,8,0q�6L   �� 5d      $��Zapisz��6(   5e      5f      5h      �� 5i      $��Anuluj��6    �� IsCancel$
 True��6&   $	��100��66      MahApps.Styles.Button.Square#��[ 5j      ��5k      �� 5l      .R�5m      �� $ 
Horizontal=�6%   .+�5n        $ Cancel=�69   $��0,0,8,0q�6G   �� 5o      $��Anuluj��6(   5p      5q      5s      5t      5u      5v      