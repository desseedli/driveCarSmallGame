
import os
import struct
from FieldFormat import FieldFormat
from Config import DataFileName

class ConfigDataGen:

    # 保存文件
    @staticmethod
    def Save(inbytes, datapath):
        datapath += DataFileName

        filedir = os.path.dirname(datapath)
        if os.path.exists(filedir) == False:
            os.makedirs(filedir)

        byteslen = len(inbytes)
        savebytes = struct.pack('i', byteslen)
        savebytes += inbytes
        file = open(datapath, 'wb+')
        file.write(savebytes)
        file.close()
    
    @staticmethod
    def Encode2Bytes(format, val):

        if format == "i":
            bytes = struct.pack(format, int(val))
        elif format == "f":
            bytes = struct.pack(format, float(val))
        elif format == "?":
            bytes = struct.pack(format, bool(val))
        elif format == "s":
            newval = val.encode()
            vallen = len(newval)
            lenbyte = struct.pack("i", vallen)

            strformat = str(vallen) + format
            valbyte = struct.pack(strformat, newval)

            bytes = lenbyte + valbyte
        return bytes

    # 文件生成函数
    @staticmethod
    def Process(fields, table):

        allbytes = bytes()

        count = 0
        '''
        for row in range(5, table.nrows):
            count += 1

            for col in range(table.ncols):
                if col in fields:
                    val = table.cell(row, col).value
                    type = table.cell(2, col).value
                    format = FieldFormat.Type2format[type][0]
                    allbytes += ConfigDataGen.Encode2Bytes(format, val)
                          
        outbytes = struct.pack('i', count)
        outbytes += allbytes
        '''

        for row_index in range(6, table.max_row + 1):
            count = count + 1
            for rol_index in range(1, table.max_column + 1):
                if rol_index in fields:
                    val = table.cell(row_index, rol_index).value
                    type = table.cell(3, rol_index).value
                    format = FieldFormat.Type2format[type][0]
                    if val is not None:
                        allbytes += ConfigDataGen.Encode2Bytes(format, val)

        ##print("count:" + str(count))
        outbytes = struct.pack('i', count)
        outbytes = outbytes + allbytes

        ##print(len(allbytes))
        return outbytes

        ##return allbytes

