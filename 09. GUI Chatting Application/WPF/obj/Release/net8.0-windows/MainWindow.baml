   M S B A M L   `   `   `  ���� >  :WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WPFWPF      WPF.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     #localclr-namespace:WPF       � Title$  
MainWindow��$	��720��$
��1280�� � WindowStyle$
 None=� � AllowsTransparency$
 True��
������� � 	Resources     BooleanToVisibilityConverter  MouseOverButtonTemplate �� 
TargetType   WPF.CustomButton ��Name ��Name � Cursor �� RelativeSource	 �� 	Converter
 �� Triggers P� Property P� Value �� 
TargetName �� Value �� Path �� Property P� 
SourceName��%"  &	        &	      7   �� �� "  .R�� .+��� $ border��#��z��._��� #��z��#��z��$��0,0,20,0q��� $ CloseButton��-   $��X��$��20��$��20��$��Right=�$��Center=�
������ 
������ 
���  ��$
��Bold"�$��5,0,0,0q�$
 Hand��y��� *CloseButtonVisible+ �� *TemplatedParent+8	   
 P�  �@J�IsMouseOver$
 True��. ��� $ border�� ��  �� *TemplatedParent+$	 Tag� � ��P� $ CloseButton�� �@J�IsMouseOver$
 True��. ��� $ CloseButton�� � �� �#202020.��� ���#303030.+�]� -   ���#333$��20��$	��Top=�.+��� $��Chatting Application��
�������$��Center=�$��10,0,0,0q���  �� Orientation$ 
Horizontal=�$��Right=�.+��� -   $	��ㅡ��$��30��
������ 
�������$��Center=�$��Center=��� -   $	��✕��$��30��
������ 
�������� $��0,20,0,0q�$��3��.+�� $��80��$	��Top=����#202020.+��� -   $��	GameStart��$	��150��$��60��$��Stretch=�$��Center=�
��� E��.���� $��30(� �� TextAlignment$ Center=�$��Center=�게임 시작�� $ 
Horizontal=�$��	0,0,100,0q�$��Right=�$��Center=�$	��400��$��60��.+��� $��60��
���  ��
��� � ��� $ Vertical=�.+��� $ 
Horizontal=�$��Stretch=�$��Stretch=�$��30��.+��� -   $��UserName��
�������$��15(����	#FF005F6B$	��200��$ Center=�$��Center=� �� Padding$ 10q�Name� $	��160�����	#FF00252A.+��� 
���  ��$��10��$��10��$
��Left=�$��3,0,0,0q��� 
�������$��15,0,0,0q�$��Center=�100�� 
�������$��10��$��10��$
��Left=�$��80,0,0,0q��� 
�������$��	95,00,0,0q�$��Center=�100�� $��Stretch=�$��Stretch=�$ 
Horizontal=�$��0,0,40,0q�$��30��.+��� ���	#FFC86400$	��0.3q�$��Stretch=�$	��120��
�������.��Shop�� ���#0F393F$	��0.3q�$��Stretch=�$	��120��
�������.��	My Info�� ���#242424$	��0.3q�$��50��
�������.��Help�� ���#242424$	��0.3q�$��Stretch=�$��50��
�������.��	Settingr� -    r�Name$ MyMediaElement�� r� Source$ 	Video.mp4?� r� LoadedBehavior$ Manual=� r� UnloadedBehavior$
 Stop=� r� Stretch$
 Fill=�$��0,80,0,0q�� $��0,0,0,33q�$��Bottom=�$
��Left=�.+��� -   $ 	ChatPanel��$	��300��$	��400��
�������$y�	Collapsed=� �� CornerRadius$ 5��._�� .+��� .+��� -	    ��Name$ 
ChatHeader��$
��Chat��$��16(�$
��Bold"�$��10q��� -
    ��Name$ ChatMessages���� -    ��Name$ 	ChatInput��$��10q�$��Bottom=�� -   $��FriendsPanel��$	��600��$	��242��$��Right=�$��Bottom=�$y�	Collapsed=�.+��� $��60��$	��Top=�$ 
Horizontal=�$��0,0,0,1q�.+��� $��60��$��60��
���  ��
��� � ��� $	��182��
���  ��$��1q�._�� $	��182��$��60��
������� � RowDefinitions �� $��2* ��� $��1* �.+�� .+��� $��	User Name��$��20(�$��0��$
��Left=�$��Center=�$��6, 0,0,0q��� -   $	��ㅡ��$��20��
������ 
���   �$	��Top=�$��Right=� J� RenderTransformOrigin$ 0.525,0&�$��0,0,0,20q��� -   $��Status��$��● Offline��
���  ��$��1��$
��Left=�$��Center=�$��6,0,0,5q�$
 Hand���� 
�������
���   �$��2q�$	��242��$	��537��$��Bottom=�$��Right=�$��	0,0,0.5,3q�._��� -   $ FriendsList��
�������$��0q����� *
Friendss+���� .R��� -   .���� $ 
Horizontal=�$��5q�.+��� $��16(�$	��100������ *Name+�� $��14(�
����������� *Status+�� -   $��
GroupPanel��
�������
���   �$��2q�$	��242��$	��600��$��Bottom=�$��Right=�$y�	Collapsed=�$��	0,0,0.5,3q�._��� $��Center=�$��Center=�.+��� $��그룹 채팅 옵션��$��20(�$��Center=�$��10q��� -   $��NoticePanel��
�������
���   �$��2q�$	��242��$	��600��$��Bottom=�$��Right=�$y�	Collapsed=�$��	0,0,0.5,3q�._��� $��Center=�$��Center=�.+��� $��알림 관리��$��20(�$��Center=�$��10q�� $��Bottom=�$��33��
���   �.+��� -     ��Name$  ChatElementsPanel��$ 
Horizontal=�$��Bottom=�$
��Left=�$��2,0,0,0q��� $ 
Horizontal=�$	��240��$��30��$��0,0,2,0q�$��Right=�.+�  -   !  Name$! Friends��   WPF.CustomButton"   Click$" Friends_Click�����#282828
�������$��80��$��0,0,1,0q�
# � Tag$# #303030��#��[ .��	Friends  -   $! Group��$" Group_Click�����#282828
�������$��80��$��0,0,1,0q�$# #303030��#��[ .��Groups  -   $! Notice��$" Notice_Click�����#282828
�������$��80��$��Notice��$��0,0,1,0q�$# #303030��#��[ 