import marshal
import traceback

def pyc_get_compiled_filename(pyc_bytes):
    data = bytes(pyc_bytes)
    for header_size in (16, 12, 8):
        try:
            code_obj = marshal.loads(data[header_size:])
            return code_obj.co_filename
        except Exception:
            with open("LE Formatter PythonLog.txt", "w") as f:
                f.writelines(traceback.format_exc())
            continue
    raise RuntimeError('Unsupported .pyc format or corrupted data')