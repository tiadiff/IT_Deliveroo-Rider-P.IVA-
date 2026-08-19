import sys

def replace_tab(content, tab_name, suffix):
    tab_start = content.find(f'Header="{tab_name}"')
    if tab_start == -1: return content
    sv_start = content.find('<ScrollViewer', tab_start)
    sv_end = content.find('</ScrollViewer>', sv_start) + len('</ScrollViewer>')
    
    old_sv = content[sv_start:sv_end]
    
    new_sv = f'''<ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Auto" Margin="0,10,0,-10">
                        <Canvas Width="800" Height="550" Margin="0,0,10,10">

                            <!-- BOX 1: Lordo -->
                            <Border Canvas.Left="0" Canvas.Top="0" Width="380" Height="120" BorderBrush="#4CAF50" BorderThickness="2" CornerRadius="10" Padding="20" Background="#F1F8E9">
                                <StackPanel>
                                    <TextBlock Text="Lordo {tab_name.replace('Statistiche ', '')}" FontSize="15" FontWeight="Bold" Foreground="#2E7D32"/>
                                    <TextBlock x:Name="tbLordo{suffix}" Text="€ 0.00" FontSize="32" FontWeight="Bold" Foreground="#2E7D32" Margin="0,10,0,0"/>
                                </StackPanel>
                            </Border>

                            <!-- BOX 2: Netto Bonificato -->
                            <Border Canvas.Left="400" Canvas.Top="0" Width="380" Height="120" BorderBrush="#2196F3" BorderThickness="2" CornerRadius="10" Padding="20" Background="#E3F2FD">
                                <StackPanel>
                                    <TextBlock Text="Netto Bonificato (Meno Ritenuta 20%)" FontSize="15" FontWeight="Bold" Foreground="#1565C0" TextWrapping="Wrap"/>
                                    <TextBlock x:Name="tbNettoBonificato{suffix}" Text="€ 0.00" FontSize="32" FontWeight="Bold" Foreground="#1565C0" Margin="0,10,0,0"/>
                                    <TextBlock Text="Questo è ciò che ricevi dall'azienda" FontSize="11" Foreground="#666" Margin="0,5,0,0" FontStyle="Italic" TextWrapping="Wrap"/>
                                </StackPanel>
                            </Border>

                            <!-- BOX 3: Spese Carburante -->
                            <Border Canvas.Left="0" Canvas.Top="140" Width="380" Height="120" BorderBrush="#FF9800" BorderThickness="2" CornerRadius="10" Padding="20" Background="#FFF3E0">
                                <StackPanel>
                                    <TextBlock Text="Spese Carburante" FontSize="15" FontWeight="Bold" Foreground="#E65100" TextWrapping="Wrap"/>
                                    <TextBlock x:Name="tbSpeseCarburante{suffix}" Text="€ 0.00" FontSize="32" FontWeight="Bold" Foreground="#E65100" Margin="0,10,0,0"/>
                                </StackPanel>
                            </Border>

                            <!-- BOX 4: Netto Reale in Tasca -->
                            <Border Canvas.Left="400" Canvas.Top="140" Width="380" Height="120" BorderBrush="#1565C0" BorderThickness="3" CornerRadius="10" Padding="20" Background="#0D47A1">
                                <StackPanel>
                                    <TextBlock Text="Netto Reale in Tasca" FontSize="15" FontWeight="Bold" Foreground="#FFFFFF" TextWrapping="Wrap"/>
                                    <TextBlock x:Name="tbNettoRealeTasca{suffix}" Text="€ 0.00" FontSize="36" FontWeight="Bold" Foreground="#FFFFFF" Margin="0,10,0,0"/>
                                    <TextBlock Text="Soldi che finiscono in tasca" FontSize="11" Foreground="#B3E5FC" Margin="0,5,0,0" FontStyle="Italic" TextWrapping="Wrap"/>
                                </StackPanel>
                            </Border>

                            <!-- BOX 5: Accumulo 730 -->
                            <Border Canvas.Left="0" Canvas.Top="280" Width="780" Height="110" BorderBrush="#9C27B0" BorderThickness="2" CornerRadius="10" Padding="20" Background="#F3E5F5">
                                <StackPanel>
                                    <TextBlock Text="Accumulo 730 - Credito (Ritenuta)" FontSize="15" FontWeight="Bold" Foreground="#6A1B9A"/>
                                    <TextBlock x:Name="tbAccumulo730{suffix}" Text="€ 0.00" FontSize="32" FontWeight="Bold" Foreground="#6A1B9A" Margin="0,10,0,0"/>
                                    <TextBlock Text="20% trattenuto - Da dichiarare al 730" FontSize="11" Foreground="#666" Margin="0,5,0,0" FontStyle="Italic"/>
                                </StackPanel>
                            </Border>
'''

    if suffix != 'Anno':
        new_sv += f'''
                            <!-- BOX 6: Efficienza -->
                            <Border Canvas.Left="0" Canvas.Top="410" Width="780" Height="110" BorderBrush="#FFC107" BorderThickness="2" CornerRadius="10" Padding="20" Background="#FFF8E1">
                                <StackPanel>
                                    <TextBlock Text="Indice di Efficienza (Spesa / Lordo)" FontSize="15" FontWeight="Bold" Foreground="#FF8F00"/>
                                    <TextBlock x:Name="tbEfficienzaVoto{suffix}" Text="VOTO: - / 10" FontSize="32" FontWeight="Bold" Foreground="#FF8F00" Margin="0,10,0,0"/>
                                    <TextBlock x:Name="tbEfficienzaTesto{suffix}" Text="Incidenza carburante: - %" FontSize="12" Foreground="#666" Margin="0,5,0,0" FontStyle="Italic"/>
                                </StackPanel>
                            </Border>
'''
    new_sv += '''
                        </Canvas>
                    </ScrollViewer>'''
                    
    return content.replace(old_sv, new_sv)

with open('MainWindow.xaml', 'r', encoding='utf-8') as f:
    c = f.read()

c = replace_tab(c, 'Statistiche Giorno', 'Giorno')
c = replace_tab(c, 'Statistiche mensili', 'Mensile')
c = replace_tab(c, 'Statistiche Annuali', 'Anno')

c = c.replace('tbLordoMensile', 'tbLordoMensile')
c = c.replace('tbNettoBonificatoMensile', 'tbNettoBonificato')
c = c.replace('tbSpeseCarburanteMensile', 'tbSpeseCarburante')
c = c.replace('tbNettoRealeTascaMensile', 'tbNettoRealeTasca')
c = c.replace('tbAccumulo730Mensile', 'tbAccumulo730')

with open('MainWindow.xaml', 'w', encoding='utf-8') as f:
    f.write(c)
