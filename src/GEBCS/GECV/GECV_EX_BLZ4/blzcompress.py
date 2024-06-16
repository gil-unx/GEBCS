import zlib
import sys
import os

def writeBLZ2File(data,f):
	blz2_file_name = f.name+'.blz2'
	with open(blz2_file_name,"wb") as new_file:
		new_file.write(data)
	print("OUTPUT BLZ2:"+blz2_file_name)
	

def blz_com(f,dsize):
	f.seek(0,0)
	def compress_chunk(size):
		deflate_compress = zlib.compressobj(9, zlib.DEFLATED, -15)
		data = deflate_compress.compress(f.read(size)) + deflate_compress.flush()
		chunksize = len(data).to_bytes(2, byteorder='little', signed=True)
		data = chunksize + data
		return data
#---------------------
	deflate_compress = zlib.compressobj(9, zlib.DEFLATED, -15)
	tail_size = dsize%0xffff
	number_part = dsize//0xffff
#---------------------
	if dsize <=0xffff:
		compressed = None
		compressed = deflate_compress.compress(f.read()) + deflate_compress.flush()
		chunksize = len(compressed).to_bytes(2, byteorder='little', signed=True)
		compressed = b"blz2" + chunksize  + compressed
		writeBLZ2File(compressed,f)
		return compressed
#---------------------
	else :
		compressed  =b""
		i=0
		head= compress_chunk(tail_size)
		compressed = compressed + head
		while i <number_part-1 :
			chunk = compress_chunk(0xffff)
			compressed = compressed + chunk
			i+=1
		last_chunk = compress_chunk(0xffff)
		compressed = b'blz2' + last_chunk + compressed
		writeBLZ2File(compressed,f)
		return compressed


if __name__ == '__main__':
	args = sys.argv
	print("GECV EX BLZ BY RANDERION(HAOJUN0823)")
	if len(args) <=1:
		print('HaoJun0823:You Need A File Path To Compress File.')
	else :
		input_file = args[1]
		with open(input_file,'rb') as read_file:
			#writeBLZ2File(binary_data,args[2])
			binary_size = os.path.getsize(read_file.name)	
			blz_com(read_file,binary_size)
		