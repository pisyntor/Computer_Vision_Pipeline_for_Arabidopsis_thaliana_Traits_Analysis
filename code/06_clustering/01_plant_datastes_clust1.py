
import numpy as np
import matplotlib.pyplot as plt

import matplotlib.patches as mpatches

import pickle
import os

from sklearn.decomposition import PCA
from sklearn.cluster import KMeans

import pandas as pd


plt.style.use('dark_background')


def altspace(start, step, count, endpoint=False, **kwargs):
   stop = start+(step*count)
   return np.linspace(start, stop, count, endpoint=endpoint, **kwargs)


def cluster_cmap(idc):
    clist = ["tomato", "mediumseagreen", "dodgerblue"]
    if hasattr(idc, '__iter__'):
        return [ clist[idx] for idx in idc ]
    else:
        return clist[idc]

ds_number_str = "1"


ecotype_clist = [
        (244, 64, 14),
        (48, 57, 249),
        (234, 250, 37),
        (24, 193, 65),
        (245, 130, 49),
        (231, 80, 219),
        (0, 182, 173),
        (115, 0, 218),
        (191, 239, 69),
        (255, 250, 200),
        (250, 190, 212),
        (66, 212, 244),
        (155, 99, 36),
        (220, 190, 255),
        (69, 158, 220),
        (255, 216, 177),
        (98, 2, 37),
        (227, 213, 12),
        (79, 159, 83),
        (170, 23, 101),
        (170, 255, 195),
        (169, 169, 169),
        (181, 111, 119),
        (144, 121, 171),
        (9, 125, 244),
        (184, 70, 30),
        (154, 35, 246),
        (229, 225, 238),
        (141, 254, 82),
        (31, 200, 209),
        (194, 217, 105),
        (91, 20, 124),
        (181, 220, 171),
        (37, 3, 193),

        (76, 112, 244),
        (145, 109, 36),
        (210, 200, 255),
        (79, 148, 220),
        (245, 226, 177),
        (88, 12, 37),
]
ecotype_clist = [ tuple([el/255.0 for el in item]) for item in ecotype_clist ]

def ecotype_color(idc, clist=ecotype_clist):
    if hasattr(idc, '__iter__'):
        return [clist[idx] for idx in idc]
    else:
        return clist[idc]


# datafolder = "data"
# datafolder = "data2"

# f = open(datafolder + "/all_stats_ds1.pickle", "rb")
# f = open(datafolder + "/all_stats_ds1_sigma2.pickle", "rb")
# f = open(datafolder + "/all_stats_ds2.pickle", "rb")
# f = open(datafolder + "/all_stats_ds2_last.pickle", "rb")

f = open("input/all_stats_ds" + ds_number_str + "_sigma2.pickle", "rb")


all_data = pickle.load(f)
f.close()

time_points = altspace(all_data[3][0], all_data[3][1], all_data[3][2])
stat_names, pair_names = all_data[4:6]
feat_names, type_names  = all_data[7:9]

all_features_list = all_data[0]
# list by plant types with np arrays of dimensiona:  time, instance, feature

all_genotype_names = all_data[8]

all_genotype_names = sorted(all_genotype_names)

# all_stats = all_stats[2]  # new format

max_n_timepoints = 0
max_n_instances = 0
tall_n_records = 0
for item in all_features_list:
    if item.shape[0] > max_n_timepoints:
        max_n_timepoints = item.shape[0]
    if item.shape[1] > max_n_instances:
        max_n_instances = item.shape[1]
    for instance_idx in range(item.shape[1]):
        not_nan_idc = ~np.isnan(item[:, instance_idx, 0])
        first_value_idx = np.where(not_nan_idc)[0][0]
        last_value_idx = np.where(not_nan_idc)[0][-1]
        # tall_n_records += len(np.where(np.isnan(item[:, instance_idx, 0]))[0])  # using feature 0, we assume all features are iether nan or not
        tall_n_records += (last_value_idx - first_value_idx + 1)  # we support nans in the middle
        pass
    # tall_n_records += item.shape[1]*item.shape[0]

all_features = np.empty([max_n_timepoints, max_n_instances, len(feat_names), len(all_features_list)])
all_features[:] = np.nan
# time, instance, feature, genotype

tall_genotypes = []
tall_instances = []
tall_timepoint_idc = []
all_features_tall = np.empty([tall_n_records, len(feat_names)])
all_features_tall[:] = np.nan

tall_filled_till_idx = 0
for item_idx, item in enumerate(all_features_list):
    all_features[0:item.shape[0], 0:item.shape[1], :, item_idx] = item
    for instance_idx in range(item.shape[1]):
        first_value_idx = np.where(~np.isnan(item[:, instance_idx, 0]))[0][0]
        last_value_idx = np.where(~np.isnan(item[:, instance_idx, 0]))[0][-1]
        slice_length = last_value_idx - first_value_idx + 1
        all_features_tall[ tall_filled_till_idx:(tall_filled_till_idx + slice_length) ] = \
            item[first_value_idx:(last_value_idx + 1), instance_idx, :]
        tall_filled_till_idx += slice_length
        tall_genotypes += [all_genotype_names[item_idx]]*slice_length
        tall_instances += [instance_idx]*slice_length
        tall_timepoint_idc += list(range(first_value_idx, last_value_idx + 1))

# save tall format to csv, works
df_tall = pd.DataFrame(all_features_tall, columns=feat_names)

df_tall["genotype"] = tall_genotypes
df_tall["instance"] = tall_instances
df_tall["timepoint_idx"] = tall_timepoint_idc
cols = df_tall.columns.to_list()
cols =  cols[-3:] + cols[0:-3]
df_tail = df_tall[cols]


notnan_mask = ~np.isnan(all_features_tall).any(axis=1)
all_features_tall_clean = all_features_tall[notnan_mask]
tall_genotypes_clean = np.array(tall_genotypes)[notnan_mask]
tall_instances_clean = np.array(tall_instances)[notnan_mask]
tall_timepoint_idc_clean = np.array(tall_timepoint_idc)[notnan_mask]


df = pd.DataFrame(all_features_tall_clean, columns=feat_names)
df["instance"] = tall_instances_clean
df["genotype"] = tall_genotypes_clean
df["time"] = tall_timepoint_idc_clean


all_instances = []
for genotype in all_genotype_names:
    instances = df[df["genotype"] == genotype]["instance"]
    for instance in instances:
        inst_full_name = genotype + "|" + str(instance)
        if inst_full_name not in all_instances:
            all_instances.append(inst_full_name)  # inpotimal, but ordered

# PCA over time points

# show new clusters obtained from distances in old PCA space

pca = PCA()
pc = pca.fit_transform(all_features_tall_clean)

print(pca.explained_variance_ratio_)

weights = pca.components_
weights = pd.DataFrame(weights, columns=feat_names, index=["pc" + str(i + 1) for i in range(pc.shape[1])])
weights.to_csv("weights_per_each_track_timepoints_PCA.csv")


# plt.rcParams['figure.figsize'] = (20, 14)

plt.rcParams['figure.figsize'] = (20, 14)

pc_df = pd.DataFrame(pc, columns=["pc" + str(i + 1) for i in range(pc.shape[1])])
pc_df["instance"] = tall_instances_clean
pc_df["genotype"] = tall_genotypes_clean
pc_df["time"] = tall_timepoint_idc_clean
pc_df["full_instance_name"] = pc_df["genotype"] + "|" + pc_df["instance"].astype(str)

cluster_column = [ cluster_info[ cluster_info["name"] == pc_df["full_instance_name"][row_idx] ].index.values[0]  for row_idx in range(pc_df.shape[0]) ]

# instance name:  pc_df["full_instance_name"][row_idx]
# cluster_info[cluster_info["name"] == "Kz-9|23"].index.values[0]

pc_df["cluster"] = cluster_column

pc_df.to_csv("tall_data_clustered_by_inter_track_distance.csv")

all_clusters = list(set(cluster_column))

for el in all_clusters:
    print("Cluster", el, "number of points:", pc_df[pc_df["cluster"] == el].shape[0])



prop_cycle = plt.rcParams['axes.prop_cycle'].by_key()['color']


# anim colored by cluster
# time anim 1_2, work
for t in range(max_n_timepoints):
    cnt = 0
    for c, cl in enumerate(all_clusters):
        pc_curr_cluster = pc_df[pc_df["cluster"] == cl]
        curr_instances = set(pc_curr_cluster["full_instance_name"])
        for curr_inst in curr_instances:
            pc_curr = pc_curr_cluster[pc_curr_cluster["full_instance_name"] == curr_inst]
            plt.plot(pc_curr["pc1"], pc_curr["pc2"],
                     # c=prop_cycle[c],
                     c=cluster_cmap(c),
                     alpha=0.08)
            if t in pc_curr["time"].values:
                plt.scatter(
                    pc_curr["pc1"][pc_curr["time"]==t].to_list()[0],
                    pc_curr["pc2"][pc_curr["time"]==t].to_list()[0],
                    #c=prop_cycle[c]
                    c = cluster_cmap(c)
                )
            cnt += 1
    print("Count, t:", cnt, t)

    plt.title("Principal components 1 and 2, colored by track cluster, time point #" + str(t))
    plt.xlabel("Principal component 1")
    plt.ylabel("Principal component 2")


    patches = []
    for idx in range(len(cl_names)):
        leg_item = mpatches.Patch(color=cluster_cmap(idx), label="cluster "+cl_names[idx])
        patches.append(leg_item)
    plt.legend(handles=patches)


    os.makedirs("PCA_1_2_clustered", exist_ok=True)
    # os.makedirs(genotype, exist_ok=True)
    plt.savefig("PCA_1_2_clustered/PCA_1_2_clustered_time_" + str(t).zfill(3) + "_.png")
    plt.clf()
    plt.close()



# 1 to 3
# time anim 1_3, work
for t in range(max_n_timepoints):
    cnt = 0
    for c, cl in enumerate(all_clusters):
        pc_curr_cluster = pc_df[pc_df["cluster"] == cl]
        curr_instances = set(pc_curr_cluster["full_instance_name"])
        for curr_inst in curr_instances:
            pc_curr = pc_curr_cluster[pc_curr_cluster["full_instance_name"] == curr_inst]
            plt.plot(pc_curr["pc1"], pc_curr["pc3"],
                     # c=prop_cycle[c],
                     c=cluster_cmap(c),
                     alpha=0.08)
            if t in pc_curr["time"].values:
                plt.scatter(
                    pc_curr["pc1"][pc_curr["time"] == t].to_list()[0],
                    pc_curr["pc3"][pc_curr["time"] == t].to_list()[0],
                    # c=prop_cycle[c]
                    c=cluster_cmap(c)
                )
            cnt += 1
    print("Count, t:", cnt, t)

    plt.title("Principal components 1 and 3, colored by track clusters, time point #" + str(t))
    plt.xlabel("Principal component 1")
    plt.ylabel("Principal component 3")

    patches = []
    for idx in range(len(cl_names)):
        leg_item = mpatches.Patch(color=cluster_cmap(idx), label="cluster " + cl_names[idx])
        patches.append(leg_item)
    plt.legend(handles=patches)

    os.makedirs("PCA_1_3_clustered", exist_ok=True)
    # os.makedirs(genotype, exist_ok=True)
    plt.savefig("PCA_1_3_clustered/PCA_1_3_clustered_time_" + str(t).zfill(3) + "_.png")
    plt.clf()
    plt.close()


# 2 to 3
# time anim 1_2, work
for t in range(max_n_timepoints):
    cnt = 0
    for c, cl in enumerate(all_clusters):
        pc_curr_cluster = pc_df[pc_df["cluster"] == cl]
        curr_instances = set(pc_curr_cluster["full_instance_name"])
        for curr_inst in curr_instances:
            pc_curr = pc_curr_cluster[pc_curr_cluster["full_instance_name"] == curr_inst]
            plt.plot(pc_curr["pc2"], pc_curr["pc3"],
                     # c=prop_cycle[c],
                     c=cluster_cmap(c),
                     alpha=0.08)
            if t in pc_curr["time"].values:
                plt.scatter(
                    pc_curr["pc2"][pc_curr["time"]==t].to_list()[0],
                    pc_curr["pc3"][pc_curr["time"]==t].to_list()[0],
                    #c=prop_cycle[c]
                    c = cluster_cmap(c)
                )
            cnt += 1
    print("Count, t:", cnt, t)

    plt.title("Principal components 2 and 3, colored by track clusters, time point #" + str(t))
    plt.xlabel("Principal component 2")
    plt.ylabel("Principal component 3")

    patches = []
    for idx in range(len(cl_names)):
        leg_item = mpatches.Patch(color=cluster_cmap(idx), label="cluster "+cl_names[idx])
        patches.append(leg_item)
    plt.legend(handles=patches)


    os.makedirs("PCA_2_3_clustered", exist_ok=True)
    # os.makedirs(genotype, exist_ok=True)
    plt.savefig("PCA_2_3_clustered/PCA_2_3_clustered_time_" + str(t).zfill(3) + "_.png")
    plt.clf()
    plt.close()




# anim painted by ecotype

# time anim 1_2, work
for t in range(max_n_timepoints):
    cnt = 0
    for c, cl in enumerate(all_genotype_names):
        pc_curr_genotype = pc_df[pc_df["genotype"] == cl]
        curr_instances = set(pc_curr_genotype["full_instance_name"])
        for curr_inst in curr_instances:
            pc_curr = pc_curr_genotype[pc_curr_genotype["full_instance_name"] == curr_inst]
            plt.plot(pc_curr["pc1"], pc_curr["pc2"],
                     # c=prop_cycle[c],
                     c=ecotype_color(c),
                     alpha=0.08)
            if t in pc_curr["time"].values:
                plt.scatter(
                    pc_curr["pc1"][pc_curr["time"]==t].to_list()[0],
                    pc_curr["pc2"][pc_curr["time"]==t].to_list()[0],
                    #c=prop_cycle[c]
                    c = ecotype_color(c)
                )
            cnt += 1
    print("Count, t:", cnt, t)

    plt.title("Principal components 1 and 2, colored by ecotypes, time point #" + str(t))
    plt.xlabel("Principal component 1")
    plt.ylabel("Principal component 2")
    # plt.legend(all_genotype_names)


    patches = []
    for idx in range(len(all_genotype_names)):
        leg_item = mpatches.Patch(color=ecotype_color(idx), label=all_genotype_names[idx])
        patches.append(leg_item)
    plt.legend(handles=patches)


    os.makedirs("PCA_1_2_ecotypes", exist_ok=True)
    # os.makedirs(genotype, exist_ok=True)
    plt.savefig("PCA_1_2_ecotypes/PCA_1_2_ecotypes_time_" + str(t).zfill(3) + "_.png")
    plt.clf()
    plt.close()



# time anim 1_3, work
for t in range(max_n_timepoints):
    cnt = 0
    for c, cl in enumerate(all_genotype_names):
        pc_curr_genotype = pc_df[pc_df["genotype"] == cl]
        curr_instances = set(pc_curr_genotype["full_instance_name"])
        for curr_inst in curr_instances:
            pc_curr = pc_curr_genotype[pc_curr_genotype["full_instance_name"] == curr_inst]
            plt.plot(pc_curr["pc1"], pc_curr["pc3"],
                     # c=prop_cycle[c],
                     c=ecotype_color(c),
                     alpha=0.08)
            if t in pc_curr["time"].values:
                plt.scatter(
                    pc_curr["pc1"][pc_curr["time"]==t].to_list()[0],
                    pc_curr["pc3"][pc_curr["time"]==t].to_list()[0],
                    #c=prop_cycle[c]
                    c = ecotype_color(c)
                )
            cnt += 1
    print("Count, t:", cnt, t)

    plt.title("Principal components 1 and 3, colored by ecotypes, time point #" + str(t))
    plt.xlabel("Principal component 1")
    plt.ylabel("Principal component 3")

    patches = []
    for idx in range(len(all_genotype_names)):
        leg_item = mpatches.Patch(color=ecotype_color(idx), label=all_genotype_names[idx])
        patches.append(leg_item)
    plt.legend(handles=patches)


    os.makedirs("PCA_1_3_ecotypes", exist_ok=True)
    # os.makedirs(genotype, exist_ok=True)
    plt.savefig("PCA_1_3_ecotypes/PCA_1_3_ecotypes_time_" + str(t).zfill(3) + "_.png")
    plt.clf()
    plt.close()


# time anim 2_3, work
for t in range(max_n_timepoints):
    cnt = 0
    for c, cl in enumerate(all_genotype_names):
        pc_curr_genotype = pc_df[pc_df["genotype"] == cl]
        curr_instances = set(pc_curr_genotype["full_instance_name"])
        for curr_inst in curr_instances:
            pc_curr = pc_curr_genotype[pc_curr_genotype["full_instance_name"] == curr_inst]
            plt.plot(pc_curr["pc2"], pc_curr["pc3"],
                     # c=prop_cycle[c],
                     c=ecotype_color(c),
                     alpha=0.08)
            if t in pc_curr["time"].values:
                plt.scatter(
                    pc_curr["pc2"][pc_curr["time"] == t].to_list()[0],
                    pc_curr["pc3"][pc_curr["time"] == t].to_list()[0],
                    # c=prop_cycle[c]
                    c=ecotype_color(c)
                )
            cnt += 1
    print("Count, t:", cnt, t)

    plt.title("Principal components 2 and 3, colored by ecotypes , time point #" + str(t))
    plt.xlabel("Principal component 2")
    plt.ylabel("Principal component 3")

    patches = []
    for idx in range(len(all_genotype_names)):
        leg_item = mpatches.Patch(color=ecotype_color(idx), label=all_genotype_names[idx])
        patches.append(leg_item)
    plt.legend(handles=patches)

    os.makedirs("PCA_2_3_ecotypes", exist_ok=True)
    # os.makedirs(genotype, exist_ok=True)
    plt.savefig("PCA_2_3_ecotypes/PCA_2_3_ecotypes_time_" + str(t).zfill(3) + "_.png")
    plt.clf()
    plt.close()




# PCA over tracks
# max_n_timepoints


max_time = 0
tracks = []
for idx in range(len(all_instances)):
    track = df_tall[df_tall["genotype"] == all_instances[idx].split("|")[0]]
    track = track[track["instance"] == int(all_instances[idx].split("|")[1])]
    current_max_time = track["timepoint_idx"].values[-1]
    if current_max_time > max_time:
        max_time = current_max_time
    tracks.append(track)


selected_genotypes = []
selected_instances = []
for idx in range(len(all_instances)):
    current_min_time = tracks[idx]["timepoint_idx"].values[0]
    selected_instances.append(tracks[idx]["instance"].values[0])
    selected_genotypes.append(tracks[idx]["genotype"].values[0])
    nan_rows_begin = pd.DataFrame(np.nan, index=range(current_min_time), columns=tracks[idx].columns)
    nan_rows_end = pd.DataFrame(np.nan, index=range(max_time - tracks[idx]["timepoint_idx"].values[-1]), columns=tracks[idx].columns)
    tracks[idx] = pd.concat([nan_rows_begin, tracks[idx], nan_rows_end]).reset_index(drop=True)
    tracks[idx] = tracks[idx].drop(columns=["instance", "genotype", "timepoint_idx"])
    tracks[idx] = tracks[idx].values.ravel()
    pass

tracks = pd.DataFrame(tracks)

track_columns = [ [ r + "_" + str(t).zfill(4) for r in df_tail.columns[3:] ]  for t in range(max_time + 1) ]
track_columns = [x for xs in track_columns for x in xs]
tracks.columns = track_columns
tracks["instance"] = selected_instances
tracks["genotypes"] = selected_genotypes


for el in track_columns:
    n_nans = tracks[el].isna().sum()
    if n_nans/track.shape[0] > 0.25:
        tracks = tracks.drop(columns=[el])

tracks = tracks.dropna()


pca = PCA()
tracks_pc = pca.fit_transform(tracks[tracks.columns[0:-2]])

print(pca.explained_variance_ratio_)

tracks_pc_df = pd.DataFrame(tracks_pc, columns=["pc" + str(i + 1) for i in range(tracks_pc.shape[1])])


# weights, works
weights = pca.components_
weights = pd.DataFrame(weights, columns=tracks.columns[0:-2], index=tracks_pc_df.columns)
weights.to_csv("weights_per_track_PCA.csv")



new_selection_instance_names = [tracks["genotypes"].values[idx] + "|" + str(tracks["instance"].values[idx]) for idx in range(tracks.shape[0])]
new_selection_old_clusters = [cluster_info[cluster_info["name"] == el].index[0]  for el in new_selection_instance_names]


plt.rcParams['figure.figsize'] = (7, 7)
plt.scatter(tracks_pc_df["pc1"], tracks_pc_df["pc2"], c=cluster_cmap(new_selection_old_clusters))
plt.title("DS" + ds_number_str + ": clusters by distances between tracks in PCA space")
plt.xlabel("Principal component 1")
plt.ylabel("Principal component 2")
# plt.legend(["cluster " + el for el in cl_names])
# plt.legend(cl_names)
patches = []
for idx in range(len(cl_names)):
    leg_item = mpatches.Patch(color=cluster_cmap(idx), label="cluster " + cl_names[idx])
    patches.append(leg_item)
plt.legend(handles=patches)
# plt.show()
plt.savefig("cl_by_dist_in_PCA_space.png")
plt.clf()
plt.close()




kmeans = KMeans(n_clusters=n_clusters, random_state=0, n_init="auto").fit(tracks_pc_df)
# kmeans = KMeans(n_clusters=n_clusters, random_state=0, n_init="auto").fit(tracks[tracks.columns[0:-2]])  # before PCA, the same

new_selection_new_clusters = kmeans.labels_

plt.rcParams['figure.figsize'] = (7, 7)
plt.scatter(tracks_pc_df["pc1"], tracks_pc_df["pc2"], c=cluster_cmap(new_selection_new_clusters))
plt.title("DS" + ds_number_str + ": clusters computed is track PCA space")
plt.xlabel("Principal component 1")
plt.ylabel("Principal component 2")
# plt.legend(["cluster " + el for el in cl_names])
# plt.legend(cl_names)

patches = []
for idx in range(len(cl_names)):
    leg_item = mpatches.Patch(color=cluster_cmap(idx), label="cluster " + cl_names[idx])
    patches.append(leg_item)
plt.legend(handles=patches)

plt.savefig("cl_after_track_PCA.png")
plt.clf()
plt.close()
# plt.show()



track_cl_map = tracks[["instance", "genotypes"]]
track_cl_map["cluster"] = new_selection_new_clusters

track_cl_map.to_csv("clustering_tracks_no_distance.csv", index=None)



print("done")