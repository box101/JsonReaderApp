namespace JsonReaderApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class Program
    {
        static void Main()
        {
            Console.WindowWidth += Console.WindowWidth / 4;

            var errorCounter = 0;
            const int testCount = 7;

            for (var testNumber = 1; testNumber <= testCount; testNumber++)
            {
                var (json, testData) = GetTestData(testNumber);

                WriteLine(ConsoleColor.White, $"\nTest #{testNumber}\n");
                
                var values = DocumentParser.GetDocumentValuesWithLocations(json);
            
                if (values.Count != testData.Count)
                {
                    errorCounter ++;
                    WriteLine(ConsoleColor.DarkRed, $"Test data records count: {testData.Count}. Parsed values count: {values.Count()}");
                }

                Console.WriteLine($"{"    JPath", -44} - {"Expected location",26} - {"Calculated location", 26} - {"     Value    ", 40}");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------");

                foreach (var (value, location, jPath) in values)
                {
                    var expectedLocation = testData.FirstOrDefault(x => x.Value.Trim().Equals(value)).Location;

                    if (expectedLocation != location)
                    {
                        errorCounter++;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    
                    var path = jPath.Replace("Nodes[0].", "").Replace(".value", "").Replace("node", "n");
                    Console.WriteLine($"{path, -44} - {expectedLocation,26} - {location, 26} - {value.Substring(0, Math.Min(37, value.Length)) + (value.Length > 37 ? "..." : ""), 40}");
                    Console.ResetColor();
                }
            }

            if (errorCounter == 0)
                WriteLine(ConsoleColor.Green, $"\nПройдено {testCount} тестов. Количество ошибок: 0. Задача решена", ConsoleColor.Yellow, " (либо множество тестов было не полным).");
            else
                WriteLine(ConsoleColor.DarkRed, "Задача не решена. Количество ошибок: " + errorCounter);
        }

        /// <summary>
        /// Тестовые данные
        /// </summary>
        /// <param name="testNumber">Номер тестового Json-документа</param>
        static (string Json, List<(string Value, string Location)> valuesWithLocaltion) GetTestData(int testNumber)
        {
            var testData = new [] {
                new List<(string Value, string Location)>
                {
                    ( "XIO(_HS_600B.State)"                                                                                             , "P0-D0-P0"  ),
                    ( "XIO(_HS_600A.State)"                                                                                             , "P0-D1-P0"  ),
                    ( "XIO(_ESD_PB_5.State)"                                                                                            , "P0-D2-P0"  ),
                    ( "XIO(_ESD_PB_4.State)"                                                                                            , "P0-D3-P0"  ),
                    ( "XIO(_ESD_PB_1.State)"                                                                                            , "P0-D4-P0"  ),
                    ( "XIO(_ESD_PB_2.State)"                                                                                            , "P0-D5-P0"  ),
                    ( "XIO(_HS_2099.State)"                                                                                             , "P0-D6-P0"  ),
                    ( "XIO(_ESD_PB_3.State)"                                                                                            , "P0-D7-P0"  ),
                    ( "ALMD(_HS_600B_Alm,_HS_600B_Alm.ProgAck,_HS_600B_Alm.ProgReset,_HS_600B_Alm.ProgDisable,_HS_600B_Alm.ProgEnable)" , "P1"        ),
                },
                new List<(string Value, string Location)>
                {
                    ( "GRT(AI_RAW_TMP.PV,AI_RAW_TMP.LSP)"  , "P0-D0-P0"  ),
                    ( "XIO(AI_RAW_TMP.LEN)"                , "P0-D1-P0"  ),
                    ( "TOF(AI_RAW_TMP.LTMROFF,?,?)"        , "P1"        ),
                    ( "TON(AI_RAW_TMP.LTMRON,?,?)"         , "P2"        ),
                },
                new List<(string Value, string Location)>
                {
                    ( "LEQ(F20[32],40.0)"     , "P0"        ),
                    ( "XIC(B50[23].8)"        , "P1-D0-P0"  ),
                    ( "LEQ(F20[32],35.0)"     , "P1-D1-P0"  ),
                    ( "TOF(AIT_TMR[21],?,?)"  , "P2"        ),
                },
                new List<(string Value, string Location)>
                {
                    ( "XIC(N100[348].12)"  , "P0-D0-P0"     ),
                    ( "XIC(T42[48].TT)	"  , "P0-D1-P0"     ),
                    ( "XIO(N100[348].13)"  , "P1"           ),
                    ( "TON(T42[48],?,?)"   , "P2-D0-P0"     ),
                    ( "OTE(N100[348].14)"  , "P2-D1-P0"     ),
                },
                new List<(string Value, string Location)>
                {
                    ( "XIC(B3[0].0)"          , "P0"        ),
                    ( "XIC(B3[2].3)"          , "P1"        ),
                    ( "XIC(_LSHH_221.State)"  , "P2"        ),
                    ( "XIC(_XY_200)"          , "P3-D0-P0"  ),
                    ( "XIC(B3[10].14)"        , "P3-D1-P0"  ),
                    ( "OTE(_XY_200)"          , "P4"        ),
                },
                new List<(string Value, string Location)>
                {
                    ( "AnalogIn(PIT_502,Rack_6:1:I.Ch5Data,PIT_502_ALMA.HHLimit,PIT_502_ALMA.HLimit,PIT_502_ALMA.LLimit,PIT_502_ALMA.LLLimit,Rack_6:1:I.Ch5Fault,B3[10].14,PIT_502_string,Rack_6_1_Running)"
                                                                                                                  , "P0"  ),
                    ( "XIC(PIT_502.FLT)"                                                                          , "P1-D0-P0"  ),
                    ( "OTE(PIT_502_ALMA.InFault)"                                                                 , "P1-D0-P1"  ),
                    ( "ALMA(PIT_502_ALMA,PIT_502.PV,B3[10].14,PIT_502_ALMA.ProgDisable,PIT_502_ALMA.ProgEnable)"  , "P1-D1-P0"  ),
                },
                new List<(string Value, string Location)>
                {
                    ( "P0"      , "P0"        ),
                    ( "P1"      , "P1"        ),
                    ( "P2"      , "P2"        ),
                    ( "P3-D0-P0", "P3-D0-P0"  ),
                    ( "P3-D1-P0", "P3-D1-P0"  ),
                    ( "P3-D1-P1", "P3-D1-P1"  ),
                    ( "P3-D2-P0", "P3-D2-P0"  ),
                    ( "P3-D2-P1-D0-P0", "P3-D2-P1-D0-P0"  ),
                    ( "P3-D2-P1-D0-P1-D0-P0", "P3-D2-P1-D0-P1-D0-P0"  ),
                    ( "P3-D2-P1-D0-P2", "P3-D2-P1-D0-P2"  ),
                    ( "P4"      , "P4"        ),
                },
            };
                
            return (File.ReadAllText($"test_{testNumber}.json"), testData[testNumber-1]);
        }

        private static void Write(params object[] outputs)
        {
            foreach (var output in outputs)
            {
                if (output is ConsoleColor color) Console.ForegroundColor = color;
                else Console.Write(output);
            }
            Console.ResetColor();
        }

        private static void WriteLine(params object[] outputs)
        {
            Write(outputs);
            Console.WriteLine();
        }
    }
}