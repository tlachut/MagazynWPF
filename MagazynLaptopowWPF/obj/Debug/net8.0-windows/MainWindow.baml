   M S B A M L   `   `   `  ����M  IMagazynLaptopowWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:MagazynLaptopowWPFMagazynLaptopowWPF  5   	   #    MagazynLaptopowWPF.MainWindow   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:MagazynLaptopowWPF     5   	   T PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3f6mah,http://metro.mahapps.com/winfx/xaml/controls  5   	   c _MahApps.Metro.IconPacks.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137ag cMahApps.Metro.IconPacks.Material, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137a?	iconPacks-http://metro.mahapps.com/winfx/xaml/iconpacks   5   	     � Title$  Magazyn Laptopów Firmy X��5
   	   $	��650��6*   $	��900��67    � WindowStartupLocation$ CenterScreen=�5   	      MahApps.Brushes.Accent(  "MahApps.Metro.Controls.MetroWindow   	GlowBrush# �   5   	   $��1q�5   	   #���   6    � 	Resources 5        AppButtonStyle �� 
TargetType   MahApps.Styles.Button.Square �� BasedOn �� Property �� Value  AccentButtonStyle  WarningButtonStyle  FilterTextBox  MahApps.Styles.TextBox*  $MahApps.Metro.Controls.TextBoxHelper  FilterLabel��%�  &	       7  &	 
    7  &	 {    7  &	 �    7  &	     �� 5   
   " ��6'   8   .�5      ��  � ��$ 4q�6'   �� 5       � ��$
 12,8q�6(   �� 5       � ��$ 36��6*   �� 5       � ��$ 36��6'   �� 5       � ��$ Center=�69   �� 5       � ��$ Center=�6;   5      �� 5   
   " ��6*   8   .�5      ��  � ��# �   6+   �� 5       � ��
 �����6+   5      �� 5"   
   " ��6+   8   .�5#      ��  � ��
 � E��6+   �� 5$       � ��
 �����6+   5%      �� 5'   
   " ��6&   8   .�5(      ��  �@ ClearTextButton$
 True��6B   �� 5)       � ��$	 150��6)   �� 5*       � ��$ Center=�69   �� 5+       � ��$ 0,0,5,0q�6'   �� 5,       � ��$ 28��6'   �� 5-       � ��$	 8,4q�6(   5.      �� 50   
   " ��6$   .�51      ��  � ��$ Center=�62   �� 52       � ��$ 10,0,5,0q�6'   53      54      .��56      � $��15q�6    � RowDefinitions 57   
   �� 58      $
��Auto �6   �� 5:      $
��Auto �6   �� 5<      $��* �6   �� 5>      $
��Auto �6   .+�5C   
   � $��0��6   	 � Orientation$	 
Horizontal=�6!   $��0,0,0,10q�6:   .+�5D      �� 
 � ToolTip$
 Odśwież dane (F5)��69   #��[ 6U   ��6   �� *LoadDataCommand+.��5E      .  (MahApps.Metro.IconPacks.PackIconMaterial     Kind$ Refresh=�6-   $��16��6<   $��16��6G   5F      �� 5H      $
 Dodaj nowy laptop��6:   #��[ 6V   ��6   �� *AddLaptopCommand+.��5I      ��  �� Orientation$ 
Horizontal=�6   .+�5J        $
 Plus=�61   $��0,0,8,0q�6=   $��Center=�6N   �� 5K      $��Dodaj��6    $��Center=�6-   5L      5M      �� 5O      $
 Edytuj zaznaczony laptop��6;   #��[ 6^   ��6   �� *EditLaptopCommand+.��5P      �� $ 
Horizontal=�6   .+�5Q        $ Pencil=�61   $��0,0,8,0q�6?   $��Center=�6P   �� 5R      $��Edytuj��6    $��Center=�6.   5S      5T      �� 5V      $
 Usuń zaznaczony laptop��6=   #��[ 6^   ��6   �� *DeleteLaptopCommand+.��5W      �� $ 
Horizontal=�6   .+�5X        $ DeleteForever=�61   $��0,0,8,0q�6F   $��Center=�6W   �� 5Y      $��Usuń��6    $��Center=�6,   5Z      5[      �� 5]      $��20��6   
������ 6#   �� 5`      $
 Eksportuj widok do CSV��67   #��[ 6X   ��6   �� *ExportCommand+.��5a      �� $ 
Horizontal=�6   .+�5b        $ FileExportOutline=�61   $��0,0,8,0q�6J   $��Center=�6[   �� 5c      $��	Eksportuj��6    $��Center=�61   5d      5e      �� 5g      $
 Importuj dane z pliku CSV��67   #��[ 6[   ��6   �� *ImportCommand+.��5h      �� $ 
Horizontal=�6   .+�5i        $ FileImportOutline=�61   $��0,0,8,0q�6J   $��Center=�6[   �� 5j      $��Importuj��6    $��Center=�60   5k      5l      5m      �� 5p   
   $��1��6     MahApps.Brushes.Gray7#���  6   $��1q�6T    �� Padding$ 15q�6h   $��0,0,0,10q�6u    	 MahApps.Brushes.Gray9#��� 	 6�   ._�5q      �  � ColumnDefinitions 5r      �� 5s      $
��Auto �6'   �� 5t      $
��Auto �6'   �� 5u      $
��Auto �6'   �� 5v      $
��Auto �6'   �� 5w      $
��Auto �6'   �� 5x      $��* �6'   �� 5y      $
��Auto �6'   .+�5|      �� $��0��6   $��Filtry:��6,   $��Center=�6;   $
��Bold"�6V   $��0,0,15,0q�6h   �� 5~      $��1��6   $��Marka:��6,   #��[ 6:   �� 5      $��2��6      	Watermark$ Wpisz markę...��5�      #��[ 5�      $	��150��6A   ��5   *   �� *FilterMarka+ �� UpdateSourceTrigger$ PropertyChanged=�5�   A   �� 5�      $��3��6   $��Model:��6,   #��[ 6:   �� 5�      $��4��6   $ Wpisz model...��5�      #��[ 5�      $	��150��6A   ��5�   *   �� *FilterModel+$ PropertyChanged=�5�   A   �� 5�      $��6��6   $
 Wyczyść filtry��6Q   #��[ 5�      $��36��6A   ��5�   )   �� *ClearFiltersCommand+.��5�        $ FilterRemoveOutline=�61   $��16��6L   $��16��6W   5�      5�      5�      &   System.Windows.Controls.DataGrid  5�   
   -   5        $��LaptopyDataGrid��5�      $��2��5�         AutoGenerateColumns$ False��5�         
IsReadOnly$
 True��5�         CanUserAddRows$ False��5�         CanUserDeleteRows$ False��5�         CanUserSortColumns$
 True��5�         SelectionMode$ Single=�5�         GridLinesVisibility$	 All=�5�         	RowHeight$ 32Q�5�      ��5�      �� *LaptopyView+ �� SelectedItem 5�      �� *SelectedLaptop+ �� Mode$ TwoWay=� J� InputBindings 5�      d� 5�       d� MouseAction$ LeftDoubleClicke�6    �� Command 6=   �� *EditLaptopCommand+ J� CommandBindings 5�      )  #System.Windows.Input.CommandBinding  5�      -   5           Command$ Delete��5�   !   6X    5�      )  #System.Windows.Controls.DataGridRow  �� Triggers! ]� 
Conditions" �� Property# �� Value$ P� Property% P� Value*  $System.Windows.Controls.DataGridCell��%T  '        '  @    �� 5�      "  6   .�5�      ��  � ��
 �����63   �� 5�       � ��
 �   �63     5�      ]� 5�      ! 5�      �� 5�   "   " �@ 
IsSelected$
# True��6B   �� 5�   "   " �@J�IsMouseOver$
# True��6C   ./�5�      ��  � ��
 ����6;   �� 5�       � ��
 �����6;   5�      P� 5�      $ �@ 
IsSelected$
% True��68   . �5�      ��  � ��
 ����6;   �� 5�       � ��
 �����6;   5�      P� 5�      $ �@J�IsMouseOver$
% True��69   . �5�      ��  � ��
 ��ح�6;   5�      5�      �� 5�      "  6   .�5�      ��  � ��
 ���� 63   �� 5�       � ��$ 0q�68   �� 5�       � ��$ 4q�60     5�      P� 5�      $ �@ 
IsSelected$
% True��68   . �5�      ��  � ��
 ���� 6;   5�      5�      5�      &   Columns& 5�      0  *System.Windows.Controls.DataGridTextColumn  5�      ,	  &System.Windows.Controls.DataGridColumn' 	  Header$' MARKA��6%   ( 	  Width5
  /System.Windows.Controls.DataGridLengthConverter$
( 1.5*
 6N   1  +System.Windows.Controls.DataGridBoundColumn)   Binding) 64   �� *Marka+*   ElementStyle* 5�      �� 5�      " ��6    .�5�      ��  �@��Padding$	 4,0q�68   �� 5�       � ��$ Center=�6B     5�      �� 5�   "   + �� Binding+ 6.   �� , �� RelativeSource, �� - �� AncestorType- DataGridRow. �� Path$. 
IsSelected�/ �� Value/ True6�   .a�5�   &   ��  � ��
 �����6C   5�   #   5�      5�        5�      $' MODEL��6%   $( 2*
 6N   ) 64   �� *Model+* 5�      �� 5�      " ��6    .�5�      ��  �@��Padding$	 4,0q�68   �� 5�       � ��$ Center=�6B     5�      �� 5�   "   + 6.   �� , �� - DataGridRow$. 
IsSelected�/ True6�   .a�5�   &   ��  � ��
 �����6C   5�   #   5�      5�        5�      $' SYSTEM OPERACYJNY��6%   $
( 1.5*
 6e   ) 6@   �� *SystemOperacyjny+* 5�      �� 5�      " ��6    .�5�      ��  �@��Padding$	 4,0q�68   �� 5�       � ��$ Center=�6B     5�      �� 5�   "   + 6.   �� , �� - DataGridRow$. 
IsSelected�/ True6�   .a�5�   &   ��  � ��
 �����6C   5�   #   5�      5�        5�      $' EKRAN (CALE)��6%   $( 1*
 6n   ) 6;   �� *RozmiarEkranu+0 �� StringFormat$0 F1��* 5�      �� 5      " ��6    .�5     ��  �@��Padding$	 4,0q�68   �� 5      � ��$ Center=�6B   �� 5      � ��$ Right=�6D     5     �� 5  "   + 6.   �� , �� - DataGridRow$. 
IsSelected�/ True6�   .a�5  &   ��  � ��
 �����6C   5  #   5	     5       5     $' ILOŚĆ (SZT.)��6%   $
( 0.8*
 6U   ) 6;   �� *Ilosc+* 5     �� 5     " ��6    .�5     ��  �@��Padding$	 4,0q�68   �� 5      � ��$ Center=�6B   �� 5      � ��$ Right=�6D     5     �� 5  "   + 6.   �� *Ilosc+/ 06H   .a�5  &   ��  � ��
 �  ��6C   �� 5  &    �@�ToolTip$ Brak na stanie!��6@   �� 5  &    � ��$
 Bold"�6C   5  #   �� 5  "   + 6.   �� *Ilosc+/ 16H   .a�5  &   ��  � ��
 � ���6C   �� 5  &    �@�ToolTip$ Ostatnia sztuka!��6@   5  #   �� 5  "   + 6.   �� , �� - DataGridRow$. 
IsSelected�/ True6�   .a�5  &   ��  � ��
 �����6C   5  #   5!     5#     1   ColumnHeaderStyle1 5'     �� 5(     =  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6   .�5)     ��  � ��
 ��iA�63   �� 5*      � ��
 �����63   �� 5+      � ��$	 8,6q�60   �� 5,      � ��$ Center=�6C   �� 5-      � ��$ 0,0,1,1q�68   �� 5.      � ��
 �����64   5/     51     �� 54  
   $��3��6   $��Bottom=�6!   #���   6<   .@�55     �� .��56     �� 
�������2 �� Padding$	2 8,0q�6N   ��6   �� *StatusMessage+6N   57     �� 58     $��Right=�6   .��5:     )  #MahApps.Metro.Controls.ProgressRing  $��18��6?   $��18��6J   
�������$��0,0,5,0q�6i   3   IsActive3 6#   �� *IsBusy+6i   5;     �� 5<     $��Right=�6   .��5=     �� 
�������$	2 8,0q�6/   �� 5>     $�Widocznych:��6    �� 5?     �6   �� *LaptopyView.Count+$ OneWay=� �� 5@     $�/��6    �� 5A     �6   �� *Laptopy.Count+$ OneWay=�5B     5C     5D     5F     5G     