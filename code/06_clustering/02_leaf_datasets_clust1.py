
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
    clist = ["tomato", "mediumseagreen", "dodgerblue", "gold", "darkorchid"]
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


df_tall = pd.read_csv("input/leaf_ds" + ds_number_str + "_aggregated.csv")

df_tall = df_tall.drop(columns=[df_tall.columns[0]])
df_tall = df_tall.drop(columns=[el for el in df_tall.columns if "stddev" in el])

tall_genotypes = list(df_tall["genotype"])
tall_instances = list(df_tall["instance"])
tall_timepoint_idc = list(df_tall["timepoint_idx"])

all_features_tall = df_tall.drop(columns=['Date', 'Time', 'DAS', 'genotype', 'instance', 'timestamp_str',
       'plant_id', 'timepoint_idx'])
feat_names = list(all_features_tall.columns)
all_features_tall = all_features_tall.values

all_genotype_names = sorted(list(set(df_tall["genotype"])))

pass

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

# show new clusters obtained from distances in old PCA space - based on all timepoints, not groubed by tracks

pca = PCA()
pc = pca.fit_transform(all_features_tall_clean)

print(pca.explained_variance_ratio_)

weights = pca.components_
weights = pd.DataFrame(weights, columns=feat_names, index=["pc" + str(i + 1) for i in range(pc.shape[1])])
weights.to_csv("weights_per_each_track_timepoints_PCA.csv")


# plt.rcParams['figure.figsize'] = (20, 14)



pc_df = pd.DataFrame(pc, columns=["pc" + str(i + 1) for i in range(pc.shape[1])])
pc_df["instance"] = tall_instances_clean
pc_df["genotype"] = tall_genotypes_clean
pc_df["time"] = tall_timepoint_idc_clean
pc_df["full_instance_name"] = pc_df["genotype"] + "|" + pc_df["instance"].astype(str)

cluster_column = [ cluster_info[ cluster_info["name"] == pc_df["full_instance_name"][row_idx] ].index.values[0]  for row_idx in range(pc_df.shape[0]) ]

# instance name:  pc_df["full_instance_name"][row_idx]
# cluster_info[cluster_info["name"] == "Kz-9|23"].index.values[0]

pc_df["cluster"] = cluster_column

pc_df.to_csv("leaves_DS" + ds_number_str + "_tall_data_clustered_by_inter_track_distance.csv")

all_clusters = list(set(cluster_column))

for el in all_clusters:
    print("Cluster", el, "number of points:", pc_df[pc_df["cluster"] == el].shape[0])



prop_cycle = plt.rcParams['axes.prop_cycle'].by_key()['color']


max_n_timepoints = max(pc_df["time"])

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

    plt.title("Principal components 1 and 2, colored by track clusters , time point #" + str(t))
    plt.xlabel("Principal component 1")
    plt.ylabel("Principal component 2")
    # plt.legend(all_genotype_names)


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

        plt.title("Principal components 1 and 3, colored by track clusters , time point #" + str(t))
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

    plt.title("Principal components 2 and 3, colored by track clusters , time point #" + str(t))
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

    plt.title("Principal components 1 and 2, colored by ecotypes , time point #" + str(t))
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

    plt.title("Principal components 1 and 3, colored by ecotypes , time point #" + str(t))
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
    # tracks[idx] = tracks[idx].drop(columns=["instance", "genotype", "timepoint_idx"])
    tracks[idx] = tracks[idx].drop(columns=[ el for el in tracks[idx].columns if el not in feat_names ])
    tracks[idx] = tracks[idx].values.ravel()
    pass

tracks = pd.DataFrame(tracks)

track_columns = [ [ r + "_" + str(t).zfill(4) for r in feat_names ]  for t in range(max_time + 1) ]
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
weights.to_csv("Leaves_DS" + ds_number_str + "_weights_combined_track_distances.csv")



new_selection_instance_names = [tracks["genotypes"].values[idx] + "|" + str(tracks["instance"].values[idx]) for idx in range(tracks.shape[0])]
new_selection_old_clusters = [cluster_info[cluster_info["name"] == el].index[0]  for el in new_selection_instance_names]


plt.rcParams['figure.figsize'] = (7, 7)
plt.scatter(tracks_pc_df["pc1"], tracks_pc_df["pc2"], c=cluster_cmap(new_selection_old_clusters))
plt.title("Leaves DS" + ds_number_str + ": clusters by distances between tracks in PCA space")
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



# 3d plot, works
fig = plt.figure(figsize=(10, 8))
ax = plt.axes(projection="3d", elev=48, azim=134)

ax.scatter(tracks_pc_df["pc1"], tracks_pc_df["pc2"], tracks_pc_df["pc3"], c=cluster_cmap(new_selection_old_clusters), marker='.') #, s=1, alpha=0.05)

ax.set_xlabel("PC 1")
ax.set_ylabel("PC 2")
ax.set_zlabel("PC 3")
ax.set_title("Leaves DS" + ds_number_str + "\nTracks clustered by distances")

# make the panes transparent
ax.xaxis.set_pane_color((1.0, 1.0, 1.0, 0.0))
ax.yaxis.set_pane_color((1.0, 1.0, 1.0, 0.0))
ax.zaxis.set_pane_color((1.0, 1.0, 1.0, 0.0))
# make the grid lines transparent
ax.xaxis._axinfo["grid"]['color'] = (1, 1, 1, 0.1)
ax.yaxis._axinfo["grid"]['color'] = (1, 1, 1, 0.1)
ax.zaxis._axinfo["grid"]['color'] = (1, 1, 1, 0.1)

plt.subplots_adjust(wspace=0.25, hspace=0.25)
plt.show()

pass





kmeans = KMeans(n_clusters=n_clusters, random_state=0, n_init="auto").fit(tracks_pc_df)
new_selection_new_clusters = kmeans.labels_

plt.scatter(tracks_pc_df["pc1"], tracks_pc_df["pc2"], c=cluster_cmap(new_selection_new_clusters))
plt.title("Leaves DS" + ds_number_str + ": clusters in track PCA space, new")
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



fig = plt.figure(figsize=(10, 8))
ax = plt.axes(projection="3d", elev=48, azim=134)

ax.scatter(tracks_pc_df["pc1"], tracks_pc_df["pc2"], tracks_pc_df["pc3"], c=cluster_cmap(new_selection_new_clusters), marker='.') #, s=1, alpha=0.05)

ax.set_xlabel("PC 1")
ax.set_ylabel("PC 2")
ax.set_zlabel("PC 3")
ax.set_title("Tracks clustered by distances in PCA space, new")

# make the panes transparent
ax.xaxis.set_pane_color((1.0, 1.0, 1.0, 0.0))
ax.yaxis.set_pane_color((1.0, 1.0, 1.0, 0.0))
ax.zaxis.set_pane_color((1.0, 1.0, 1.0, 0.0))
# make the grid lines transparent
ax.xaxis._axinfo["grid"]['color'] = (1, 1, 1, 0.1)
ax.yaxis._axinfo["grid"]['color'] = (1, 1, 1, 0.1)
ax.zaxis._axinfo["grid"]['color'] = (1, 1, 1, 0.1)

plt.subplots_adjust(wspace=0.25, hspace=0.25)
plt.show()




track_cl_map = tracks[["instance", "genotypes"]]
track_cl_map["cluster"] = new_selection_new_clusters

track_cl_map.to_csv("clustering_tracks_no_distance.csv", index=None)


print("done")