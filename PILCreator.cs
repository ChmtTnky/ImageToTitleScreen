namespace PILCreator
{
    public static class PILCreator
    {
        public static bool GeneratePIL(string[] pim_paths, string output_path="OUTPUT.PIL")
        {
            // default PIL header is 16 bytes
            // total_size = 16 + sum_of_all_pims
            // pim_size = 16 + pim_data.length
            int total_size = 0x10;
            byte[][] pim_file_data = new byte[pim_paths.Length][];
            for (int i = 0; i < pim_paths.Length; i++)
            {
                pim_file_data[i] = File.ReadAllBytes(pim_paths[i]);
                int pim_length = 16 + pim_file_data[i].Length;
                total_size += pim_length;
            }
            byte[] pil_data = new byte[total_size];

            // write first 8 bytes of header, then file name with 4 byte alignment, then the rest of the data starting at 0x10 (after the header)
            // repeat for every pim file
            pil_data[0] = (byte)pim_paths.Length;
            int index = 0x10;
            for (int i = 0; i < pim_paths.Length; i++)
            {
                pim_file_data[i].Take(8).ToArray().CopyTo(pil_data, index);
                index += 8;
                for (int j = 0; j < Path.GetFileNameWithoutExtension(pim_paths[i]).Length; j++)
                    pil_data[index + j] = (byte)Path.GetFileNameWithoutExtension(pim_paths[i])[j];
                index += 24;
                for (int k = 16; k < pim_file_data[i].Length; k++)
                    pil_data[index++] = pim_file_data[i][k];
            }
            File.WriteAllBytes(output_path, pil_data);

            if (File.Exists(output_path))
                return true;
            return false;
        }
    }
}
