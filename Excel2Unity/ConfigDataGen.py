
import os
import struct
from FieldFormat import FieldFormat
from Config import DataFileName
import re

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
    def Encode2Bytes(format, val, enum_list):
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
        elif format == 'e':
            bytes = struct.pack("i", enum_list.index(val))
        return bytes

    # 文件生成函数
    @staticmethod
    def Process(fields, table):

        allbytes = bytes()

        count = 0
        enum_res_dict = {}
        enum_list_one = []
        format = ''
        for row_index in range(6, table.max_row + 1):
            count = count + 1
            for rol_index in range(1, table.max_column + 1):
                if rol_index in fields:
                    val = table.cell(row_index, rol_index).value
                    type = table.cell(3, rol_index).value
                    if "enum" in type:
                        pattern = r'\((.*?)\)'
                        pattern2 = r'enum:(\w+)'
                        matches = re.findall(pattern, type)
                        enum_name = re.search(pattern2, type).group(1)
                        if matches:
                            enum_list_one = matches[0].split(',')
                            if len(enum_list_one) > 1:
                                enum_res_dict[enum_name] = enum_list_one
                            format = FieldFormat.Type2format["enum"][0]
                    else:
                        format = FieldFormat.Type2format[type][0]
                    if val is not None:
                        allbytes += ConfigDataGen.Encode2Bytes(format, val, enum_list_one)

        outbytes = struct.pack('i', count)
        outbytes = outbytes + allbytes

        return outbytes, enum_res_dict

